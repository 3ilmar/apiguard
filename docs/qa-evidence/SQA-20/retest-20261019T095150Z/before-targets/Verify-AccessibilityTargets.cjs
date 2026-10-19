#!/usr/bin/env node
'use strict';

// Supplement the full regression runner with repeatable checks of the original
// label and contrast defects. No form is submitted and no API check is run.
const fs = require('node:fs');
const path = require('node:path');
const { execFileSync } = require('node:child_process');
const { chromium } = require(require.resolve('playwright', { paths: [process.env.SQA20_NODE_MODULES || __dirname] }));
const { availability, scanPage } = require('./browser-accessibility.cjs');
if (!process.argv[2]) throw new Error('Supply a new output directory as the first argument.');
const output = path.resolve(process.argv[2]);
if (fs.existsSync(output)) throw new Error('Output already exists; use a new directory to preserve evidence.');
fs.mkdirSync(output, { recursive: true });
const root = path.resolve(__dirname, '../..');
const baseUrl = 'http://localhost:51920';
const git = args => execFileSync('git', args, { cwd: root, encoding: 'utf8' }).trim();
const save = (name, value) => fs.writeFileSync(path.join(output, name), JSON.stringify(value, null, 2) + '\n');
const expectedNames = {
  Name: 'Name', Path: 'Path', Method: 'Method', ExpectedStatusCode: 'Expected HTTP status',
  MaxResponseTimeMs: 'Max response time (ms)', ExpectedContentType: 'Expected content type',
  RequiresAuthentication: 'Requires authentication', status: 'Status', priority: 'Priority', assignedTo: 'Assigned to'
};
const routes = [
  ['orders', '/Apis/Details/20200000-0000-0000-0000-000000000001', 7],
  ['catalogue', '/Apis/Details/20200000-0000-0000-0000-000000000002', 7],
  ['defects', '/Defects', 12]
];
const summary = {
  startedAtUtc: new Date().toISOString(), branch: git(['branch', '--show-current']), baseCommit: git(['rev-parse', 'HEAD']),
  sourceState: git(['status', '--short']), applicationSourceChanges: git(['status', '--short', '--', 'SecureApiQualityPlatform.Web']),
  node: process.version, axe: availability, baseUrl, method: 'Installed headless Chrome/Edge; native label clicks and keyboard Tab; unsent checkbox values restored; computed text/background colours.',
  criteria: 'All 26 original controls per browser/viewport have unique IDs, associated purpose labels, matching accessibility snapshots when supported, and native label behaviour. Applicable target text contrast states meet 4.5:1. Raw axe naming/contrast rules must pass.',
  limitations: ['Automated technical retest only; no human acceptance or full WCAG conformance is claimed.', 'Noninteractive URL code has no keyboard-focus or active state; those states are N/A without modifying its semantics.', 'Contrast calculation supports solid alpha-composited CSS backgrounds; background images or ancestor opacity make a measurement BLOCKED.'],
  browsers: []
};
fs.copyFileSync(__filename, path.join(output, path.basename(__filename)));

async function checkLabels(page) {
  const controls = page.locator('form[action*="AddEndpoint"] input:not([type="hidden"]), form[action*="AddEndpoint"] select, form[action*="Update"] select, form[action*="Update"] input[name="assignedTo"]');
  const results = [];
  for (let index = 0; index < await controls.count(); index++) {
    const control = controls.nth(index);
    const fact = await control.evaluate(element => ({
      id: element.id, name: element.name, type: element.type,
      uniqueId: Boolean(element.id) && [...document.querySelectorAll('[id]')].filter(other => other.id === element.id).length === 1,
      labels: [...(element.labels || [])].map(label => ({ text: label.textContent.trim().replace(/\s+/g, ' '), htmlFor: label.htmlFor, controlsThisElement: label.control === element })),
      associated: Boolean(element.labels?.length) && [...element.labels].every(label => label.control === element)
    }));
    fact.expectedName = expectedNames[fact.name] || null;
    if (typeof control.ariaSnapshot === 'function') {
      fact.ariaSnapshot = await control.ariaSnapshot();
      const match = fact.ariaSnapshot.split('\n')[0].match(/^-\s+\w+\s+"((?:\\.|[^"\\])*)"/);
      fact.accessibilitySnapshotName = match ? JSON.parse('"' + match[1] + '"') : null;
      fact.accessibilitySnapshotPassed = fact.accessibilitySnapshotName === fact.expectedName;
    } else fact.accessibilitySnapshotAvailability = 'Unavailable in this Playwright version; DOM label association remains checked.';
    const label = control.locator('xpath=..').locator('label').first();
    fact.labelClick = { available: await label.count() > 0 };
    if (fact.labelClick.available) {
      // Move focus away first so an unrelated label cannot appear to succeed.
      for (let step = 0; step < 2 && await control.evaluate(element => document.activeElement === element); step++) await page.keyboard.press('Tab');
      const before = await page.locator('input[type="checkbox"]').evaluateAll(elements => elements.map(element => ({ id: element.id, checked: element.checked })));
      await label.click();
      await page.waitForTimeout(180);
      fact.labelClick.focusedCorrectControl = await control.evaluate(element => document.activeElement === element);
      if (fact.type === 'checkbox') {
        const after = await page.locator('input[type="checkbox"]').evaluateAll(elements => elements.map(element => ({ id: element.id, checked: element.checked })));
        fact.labelClick.checkboxBefore = before;
        fact.labelClick.checkboxAfter = after;
        fact.labelClick.onlyIntendedCheckboxToggled = before.length === after.length && before.every((item, offset) => item.id === after[offset].id && (item.id === fact.id ? item.checked !== after[offset].checked : item.checked === after[offset].checked));
        if (before.some((item, offset) => item.checked !== after[offset].checked)) {
          await label.click();
          fact.labelClick.checkboxRestored = JSON.stringify(before) === JSON.stringify(await page.locator('input[type="checkbox"]').evaluateAll(elements => elements.map(element => ({ id: element.id, checked: element.checked }))));
        } else fact.labelClick.checkboxRestored = true;
      }
    }
    fact.passed = fact.uniqueId && fact.associated && fact.labels.some(label => label.text === fact.expectedName) &&
      fact.accessibilitySnapshotPassed !== false && fact.labelClick.focusedCorrectControl === true &&
      (fact.type !== 'checkbox' || fact.labelClick.onlyIntendedCheckboxToggled === true && fact.labelClick.checkboxRestored === true);
    results.push(fact);
  }
  return results;
}

async function measureContrast(target) {
  return target.evaluate(element => {
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = 1;
    const context = canvas.getContext('2d', { willReadFrequently: true });
    const rgba = css => {
      context.clearRect(0, 0, 1, 1);
      context.fillStyle = css;
      context.fillRect(0, 0, 1, 1);
      return [...context.getImageData(0, 0, 1, 1).data].map((value, index) => index === 3 ? value / 255 : value);
    };
    const composite = (front, back) => {
      const alpha = front[3] + back[3] * (1 - front[3]);
      return [0, 1, 2].map(index => (front[index] * front[3] + back[index] * back[3] * (1 - front[3])) / alpha).concat(alpha);
    };
    const ancestors = [];
    for (let current = element; current; current = current.parentElement) {
      const style = getComputedStyle(current);
      ancestors.unshift({ tag: current.tagName, className: current.className, backgroundColor: style.backgroundColor, backgroundImage: style.backgroundImage, opacity: style.opacity });
    }
    let background = [255, 255, 255, 1];
    for (const item of ancestors) background = composite(rgba(item.backgroundColor), background);
    const style = getComputedStyle(element);
    const foreground = composite(rgba(style.color), background);
    const luminance = color => color.slice(0, 3).map(value => value / 255).map(value => value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4).reduce((total, value, index) => total + value * [0.2126, 0.7152, 0.0722][index], 0);
    const foregroundLuminance = luminance(foreground), backgroundLuminance = luminance(background);
    const supported = ancestors.every(item => item.backgroundImage === 'none' && Number(item.opacity) === 1);
    const ratio = (Math.max(foregroundLuminance, backgroundLuminance) + 0.05) / (Math.min(foregroundLuminance, backgroundLuminance) + 0.05);
    return {
      text: element.textContent.trim(), color: style.color, compositeForeground: foreground, compositeBackground: background,
      fontSize: style.fontSize, fontWeight: style.fontWeight, ratio, required: 4.5, supported,
      passed: supported && ratio >= 4.5, status: supported ? ratio >= 4.5 ? 'PASS' : 'FAIL' : 'BLOCKED', ancestors,
      state: { hover: element.matches(':hover'), focus: element.matches(':focus'), focusVisible: element.matches(':focus-visible'), active: element.matches(':active') },
      focusIndicator: { outline: style.outline, outlineOffset: style.outlineOffset, boxShadow: style.boxShadow, borderColor: style.borderColor }
    };
  });
}

async function checkContrast(page, url, isLink, prefix) {
  const selector = isLink ? 'a[href="/Reports/ExportDefectsCsv"]' : 'h1 + code';
  const target = page.locator(selector);
  const results = [];
  for (const state of ['resting', 'hover', 'focus', 'active']) {
    if (!isLink && ['focus', 'active'].includes(state)) {
      results.push({ state, status: 'N/A', reason: 'Noninteractive code text has no native focus/active state.' });
      continue;
    }
    await page.goto(url, { waitUntil: 'networkidle' });
    await page.mouse.move(0, 0);
    if (state === 'hover' || state === 'active') await target.hover();
    if (state === 'focus') {
      let reached = false;
      for (let step = 0; step < 120; step++) {
        await page.keyboard.press('Tab');
        if (await target.evaluate(element => document.activeElement === element)) { reached = true; break; }
      }
      if (!reached) { results.push({ state, status: 'FAIL', reason: 'Target was not reached through bounded native Tab traversal.' }); continue; }
    }
    if (state === 'active') await page.mouse.down();
    try {
      await page.waitForTimeout(200);
      const fact = await measureContrast(target);
      fact.requestedState = state;
      const stateApplied = state === 'resting' ? !fact.state.hover && !fact.state.focus && !fact.state.active : state === 'focus' ? fact.state.focus && fact.state.focusVisible : fact.state[state];
      fact.stateApplied = stateApplied;
      if (!stateApplied) { fact.passed = false; fact.status = 'FAIL'; }
      fact.screenshot = `${prefix}-${state}.png`;
      await page.screenshot({ path: path.join(output, fact.screenshot), fullPage: false, animations: 'disabled' });
      results.push(fact);
    } finally {
      if (state === 'active') {
        // Release outside the anchor to avoid activating the export link.
        await page.mouse.move(0, 0);
        await page.mouse.up();
      }
    }
  }
  return results;
}

(async () => {
  if (!availability.available) {
    summary.status = 'BLOCKED';
    summary.reason = 'Pinned axe dependency unavailable; no browser checks executed.';
  } else {
    for (const [name, executablePath] of [
      ['chrome', process.env.SQA20_CHROME || 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe'],
      ['edge', process.env.SQA20_EDGE || 'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe']
    ]) {
      const record = { browser: name, executablePath, pages: [], events: [] };
      summary.browsers.push(record);
      let browser;
      try {
        browser = await chromium.launch({ executablePath, headless: true });
        record.version = browser.version();
        const context = await browser.newContext({ viewport: { width: 1440, height: 900 }, deviceScaleFactor: 1, locale: 'en-NZ', timezoneId: 'Pacific/Auckland' });
        await context.addCookies([{ name: 'apiguard_role', value: 'Admin', url: baseUrl + '/', httpOnly: true, sameSite: 'Lax' }]);
        await context.route('**/*', route => {
          const request = route.request();
          const allowed = (new URL(request.url()).origin === baseUrl || request.url() === 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css') && ['GET', 'HEAD'].includes(request.method());
          if (!allowed) record.events.push({ atUtc: new Date().toISOString(), type: 'network-guard', method: request.method(), url: request.url() });
          return allowed ? route.continue() : route.abort('blockedbyclient');
        });
        const page = await context.newPage();
        page.on('pageerror', error => record.events.push({ type: 'pageerror', error: String(error) }));
        for (const [viewportName, viewport] of [['desktop', { width: 1440, height: 900 }], ['narrow', { width: 375, height: 812 }]]) {
          await page.setViewportSize(viewport);
          for (const [pageName, route, expectedControlCount] of routes) {
            const prefix = `${name}-${viewportName}-${pageName}`;
            const response = await page.goto(baseUrl + route, { waitUntil: 'networkidle' });
            const stylesheetFacts = await page.locator('link[rel="stylesheet"]').evaluateAll(elements => elements.map(element => ({ href: element.href, loaded: Boolean(element.sheet) })));
            const rawScan = await scanPage(page);
            save(`axe-${prefix}.json`, rawScan);
            const labels = await checkLabels(page);
            const contrast = await checkContrast(page, baseUrl + route, pageName === 'defects', prefix);
            const relevantAxeViolations = rawScan.violations.filter(rule => ['label', 'select-name', 'color-contrast'].includes(rule.id));
            const pageRecord = { page: pageName, viewportName, viewport, atUtc: new Date().toISOString(), httpStatus: response?.status(), stylesheetFacts, expectedControlCount, labels, contrast, axeEvidence: `axe-${prefix}.json`, relevantAxeViolations: relevantAxeViolations.map(rule => ({ id: rule.id, targets: rule.nodes.map(node => node.target) })) };
            const stylesAvailable = stylesheetFacts.length === 2 && stylesheetFacts.every(item => item.loaded);
            pageRecord.status = pageRecord.httpStatus !== 200 || !stylesAvailable || contrast.some(item => item.status === 'BLOCKED') ? 'BLOCKED' :
              labels.length === expectedControlCount && labels.every(item => item.passed) && contrast.every(item => ['PASS', 'N/A'].includes(item.status)) && !relevantAxeViolations.length ? 'PASS' : 'FAIL';
            record.pages.push(pageRecord);
            save(`${prefix}.json`, pageRecord);
            console.log(`${new Date().toISOString()} ${prefix}: ${pageRecord.status}`);
          }
        }
        record.status = record.pages.some(item => item.status === 'FAIL') ? 'FAIL' : record.pages.some(item => item.status === 'BLOCKED') || record.events.some(item => item.type === 'pageerror') ? 'BLOCKED' : 'PASS';
      } catch (error) { record.status = 'BLOCKED'; record.error = String(error.stack || error); }
      finally { if (browser) await browser.close(); }
    }
    summary.status = summary.browsers.some(item => item.status === 'FAIL') ? 'FAIL' : summary.browsers.some(item => item.status === 'BLOCKED') ? 'BLOCKED' : 'PASS';
  }
  summary.completedAtUtc = new Date().toISOString();
  save('accessibility-targets.json', summary);
  console.log(JSON.stringify({ status: summary.status, browsers: summary.browsers.map(item => ({ browser: item.browser, version: item.version, status: item.status })) }));
  if (summary.status !== 'PASS') process.exitCode = 1;
})().catch(error => { summary.status = 'BLOCKED'; summary.error = String(error.stack || error); save('accessibility-targets.json', summary); console.error(error); process.exitCode = 1; });
