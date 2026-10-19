#!/usr/bin/env node
'use strict';

// Runs the current MVC application through installed Chrome and Edge binaries.
// Browser contexts are fresh; no user profile or original application store is used.
const fs = require('node:fs');
const path = require('node:path');
const os = require('node:os');
const crypto = require('node:crypto');
const { execFileSync } = require('node:child_process');
const moduleRoot = process.env.SQA20_NODE_MODULES;
const playwrightPath = require.resolve('playwright', { paths: moduleRoot ? [moduleRoot] : [__dirname] });
const { chromium } = require(playwrightPath);
const { scanPage, availability: axeAvailability } = require('./browser-accessibility.cjs');
const { testKeyboard } = require('./browser-keyboard.cjs');
const { inspectLayout } = require('./browser-layout.cjs');
const argument = process.argv.indexOf('--output');
if (argument < 0 || !process.argv[argument + 1]) throw new Error('Supply --output <new session directory>.');
const output = path.resolve(process.argv[argument + 1]);
const root = path.resolve(__dirname, '../..');
const baseUrl = 'http://localhost:51920';
// Matches CookieRoleProvider.CookieName. This user-selected demo role is not
// authentication; it permits read-only inspection of the real Admin-only form.
const demoRole = { cookieName: 'apiguard_role', value: 'Admin', scope: baseUrl,
  purpose: 'Read-only registration/form inspection in this isolated browser context; application role filters remain active.' };
let startedAt = new Date().toISOString();
const finalizeOnly = process.argv.includes('--finalize-only');
const prefix = name => name === 'chrome' ? 'CH' : 'ED';
const titles = ['Compatibility', 'Forward keyboard', 'Reverse keyboard', 'Responsive layout', 'Severity presentation', 'Tables and long content', 'Results CSV', 'Defects CSV'];
const cases = [
  { name: 'dashboard', url: '/', heading: 'API Quality Dashboard' },
  { name: 'apis', url: '/Apis', heading: 'Registered APIs' },
  { name: 'orders', url: '/Apis/Details/20200000-0000-0000-0000-000000000001', heading: 'SQA-20 Orders, "review"' },
  { name: 'catalogue', url: '/Apis/Details/20200000-0000-0000-0000-000000000002', heading: 'SQA-20 Catalogue, "healthy"' },
  { name: 'defects', url: '/Defects', heading: 'Defect Register' },
  { name: 'registration', url: '/Apis/Create', heading: 'Register API' }
];
const browsers = [
  { name: 'chrome', product: 'Google Chrome', executablePath: process.env.SQA20_CHROME || 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe' },
  { name: 'edge', product: 'Microsoft Edge', executablePath: process.env.SQA20_EDGE || 'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe' }
];
const json = (file, value) => fs.writeFileSync(file, JSON.stringify(value, null, 2) + '\n');
const relative = file => path.relative(output, file).replaceAll('\\', '/');
const git = args => execFileSync('git', args, { cwd: root, encoding: 'utf8' }).trim();
const log = message => {
  const line = `${new Date().toISOString()} ${message}`;
  console.log(line);
  fs.appendFileSync(path.join(output, 'execution.log'), line + '\n');
};
const result = (browser, index, status, actual, evidence = [], limitations = []) => ({
  id: `SQA20-${prefix(browser)}${String(index).padStart(2, '0')}`,
  browser, test: titles[index - 1], status, actual, evidence: [...new Set(evidence)],
  executionMethod: 'Playwright automation using installed browser executable; headless, isolated context',
  limitations, executedAtUtc: new Date().toISOString()
});
fs.mkdirSync(output, { recursive: true });
if (!finalizeOnly && fs.existsSync(path.join(output, 'browser-execution.json'))) throw new Error('Refusing to overwrite a previous browser execution. Choose a new output directory.');
if (git(['branch', '--show-current']) !== 'fix/sqa20-accessibility-postmerge') throw new Error('Wrong branch; no tests executed.');
const applicationSourceChanges = git(['status', '--short', '--', 'SecureApiQualityPlatform.Web']);
const metadata = finalizeOnly ? JSON.parse(fs.readFileSync(path.join(output, 'environment.json'), 'utf8')) : {
  startedAtUtc: startedAt, branch: git(['branch', '--show-current']), baseCommit: git(['rev-parse', 'HEAD']),
  sourceState: git(['status', '--short']), node: process.version, os: { platform: os.platform(), release: os.release(), architecture: os.arch(), cpu: os.cpus()[0]?.model, logicalCpus: os.cpus().length },
  playwright: require(path.join(path.dirname(playwrightPath), 'package.json')).version,
  playwrightPath, baseUrl, headless: true, viewportDesktop: { width: 1440, height: 900 }, viewportNarrow: { width: 375, height: 812 }, deviceScaleFactor: 1,
  axe: axeAvailability, originalApplicationCodeModified: Boolean(applicationSourceChanges), applicationSourceChanges, demoRole,
  networkPolicy: 'Only localhost:51920 GET/HEAD and original Bootstrap stylesheet URL allowed by browser context. No API checks or form submissions.',
  humanTesting: 'No human SQA-20 execution or acceptance is performed by this runner; separate human evidence is not reassessed.'
};
if (!finalizeOnly) json(path.join(output, 'environment.json'), metadata);
else startedAt = metadata.startedAtUtc;
const sourceDirectory = path.join(output, 'runner-source');
fs.mkdirSync(sourceDirectory, { recursive: true });
for (const file of finalizeOnly ? [] : ['Run-BrowserChecks.cjs', 'browser-keyboard.cjs', 'browser-layout.cjs', 'browser-accessibility.cjs']) {
  fs.copyFileSync(path.join(__dirname, file), path.join(sourceDirectory, file));
}
if (!finalizeOnly) fs.copyFileSync(path.join(__dirname, 'axe/dependency.json'), path.join(sourceDirectory, 'axe-dependency.json'));

async function severityFacts(page) {
  return page.evaluate(() => {
    const metrics = Object.fromEntries([...document.querySelectorAll('.metric-card')].map(card => [card.querySelector('span')?.textContent.trim(), card.querySelector('strong')?.textContent.trim()]));
    const rows = [...document.querySelectorAll('.severity-row')].map(row => {
      const progress = row.querySelector('[role="progressbar"]');
      const bar = row.querySelector('.progress-bar');
      const label = row.querySelector('span')?.textContent.trim();
      const count = row.querySelector('strong')?.textContent.trim();
      const progressRect = progress.getBoundingClientRect(), barRect = bar.getBoundingClientRect();
      return { label, count, accessibleName: progress.getAttribute('aria-label'), now: progress.getAttribute('aria-valuenow'), max: progress.getAttribute('aria-valuemax'), percent: 100 * barRect.width / progressRect.width, textColor: getComputedStyle(row).color, visible: progressRect.width > 0 && row.getBoundingClientRect().height > 0 };
    });
    const expected = ['Critical', 'High', 'Medium', 'Low'];
    return { metrics, rows, passed: metrics['Open defects'] === '4' && metrics['High/Critical open'] === '2' && rows.length === 4 && rows.every((row, index) => row.label === expected[index] && row.count === '1' && row.accessibleName === `${row.label} open defects` && row.now === '1' && row.max === '4' && Math.abs(row.percent - 25) < 1 && row.visible) };
  });
}

async function verifyScrollableRegions(page, keyboard, scan) {
  const findings = scan?.violations.filter(rule => rule.id === 'scrollable-region-focusable') || [];
  const results = [];
  for (const node of findings.flatMap(rule => rule.nodes)) {
    const proof = { target: node.target, reachedByTab: false, reachedByShiftTab: false, keyboardScrollWorks: false };
    // This fixture has light DOM. Keep any unexpected selector form unresolved.
    if (node.target.length === 1 && typeof node.target[0] === 'string') {
      const locator = page.locator(node.target[0]);
      if (await locator.count() === 1) {
        const wrapper = await locator.evaluate(element => ({
          key: `dom-${[...document.querySelectorAll('*')].indexOf(element)}`,
          isTableWrapper: element.matches('.table-responsive'),
          clientWidth: element.clientWidth, scrollWidth: element.scrollWidth,
          tabIndex: element.tabIndex, attributeTabIndex: element.getAttribute('tabindex')
        }));
        Object.assign(proof, wrapper);
        proof.reachedByTab = keyboard.forward.some(event => event.active.key === wrapper.key);
        proof.reachedByShiftTab = keyboard.reverse.some(event => event.active.key === wrapper.key);
        if (wrapper.isTableWrapper && wrapper.scrollWidth > wrapper.clientWidth && proof.reachedByTab && proof.reachedByShiftTab) {
          for (let step = 0; step < 120; step++) {
            await page.keyboard.press('Tab');
            const activeKey = await page.evaluate(() => `dom-${[...document.querySelectorAll('*')].indexOf(document.activeElement)}`);
            if (activeKey === wrapper.key) {
              proof.scrollBefore = await locator.evaluate(element => element.scrollLeft);
              await page.keyboard.press('ArrowRight');
              await page.waitForTimeout(180);
              proof.scrollAfter = await locator.evaluate(element => element.scrollLeft);
              proof.keyboardScrollWorks = proof.scrollAfter > proof.scrollBefore;
              break;
            }
          }
        }
      }
    }
    proof.unconfirmedScannerFlag = proof.reachedByTab && proof.reachedByShiftTab && proof.keyboardScrollWorks;
    results.push(proof);
  }
  return results;
}

async function executeBrowser(configuration) {
  const directory = path.join(output, configuration.name);
  fs.mkdirSync(directory, { recursive: true });
  fs.mkdirSync(path.join(directory, 'downloads'), { recursive: true });
  const summary = { browser: configuration.name, product: configuration.product, executablePath: configuration.executablePath, startedAtUtc: new Date().toISOString(), pages: [], navigation: [], keyboard: [], layouts: [], accessibility: [], severity: [], downloads: [], results: [] };
  const events = [];
  let phase = 'launch';
  let browser;
  try {
    if (!fs.existsSync(configuration.executablePath)) throw new Error(`Installed executable not found: ${configuration.executablePath}`);
    browser = await chromium.launch({ executablePath: configuration.executablePath, headless: true, timeout: 30000 });
    summary.version = browser.version();
    summary.substitution = false;
    log(`${configuration.product} ${summary.version} launched from ${configuration.executablePath}`);
    const context = await browser.newContext({ viewport: metadata.viewportDesktop, deviceScaleFactor: 1, acceptDownloads: true, locale: 'en-NZ', timezoneId: 'Pacific/Auckland', colorScheme: 'light' });
    await context.addCookies([{ name: demoRole.cookieName, value: demoRole.value,
      url: baseUrl + '/', httpOnly: true, sameSite: 'Lax', secure: false }]);
    summary.demoRole = demoRole;
    await context.route('**/*', route => {
      const request = route.request(), url = new URL(request.url());
      const local = url.origin === baseUrl;
      const stylesheet = request.url() === 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css';
      if ((local || stylesheet) && ['GET', 'HEAD'].includes(request.method())) return route.continue();
      events.push({ atUtc: new Date().toISOString(), phase, type: 'network-guard', url: request.url(), method: request.method() });
      return route.abort('blockedbyclient');
    });
    const page = await context.newPage();
    page.setDefaultTimeout(15000);
    page.on('console', message => { if (['error', 'warning'].includes(message.type())) events.push({ atUtc: new Date().toISOString(), phase, type: `console-${message.type()}`, text: message.text() }); });
    page.on('pageerror', error => events.push({ atUtc: new Date().toISOString(), phase, type: 'pageerror', text: String(error) }));
    page.on('requestfailed', request => events.push({ atUtc: new Date().toISOString(), phase, type: 'requestfailed', url: request.url(), method: request.method(), error: request.failure()?.errorText }));
    page.on('response', response => events.push({ atUtc: new Date().toISOString(), phase, type: 'response', url: response.url(), status: response.status(), contentType: response.headers()['content-type'] || '' }));
    summary.userAgent = await page.evaluate(() => navigator.userAgent);
    summary.preconditions = 'Existing isolated host; fixed synthetic fixture; explicit Admin demo-role cookie in isolated context; no form submissions or API execution. Default no-cookie Viewer behavior is unchanged.';
    for (const viewportName of ['desktop', 'narrow']) {
      await page.setViewportSize(viewportName === 'desktop' ? metadata.viewportDesktop : metadata.viewportNarrow);
      for (const item of cases) {
        phase = `${viewportName}-${item.name}`;
        log(`${configuration.name}: ${phase}`);
        const response = await page.goto(baseUrl + item.url, { waitUntil: 'networkidle', timeout: 30000 });
        const facts = await page.evaluate(() => ({
          title: document.title, heading: document.querySelector('h1')?.textContent.trim(),
          viewport: { width: innerWidth, height: innerHeight, dpr: devicePixelRatio },
          stylesheets: [...document.querySelectorAll('link[rel="stylesheet"]')].map(link => ({ href: link.href, loaded: !!link.sheet })),
          controls: [...document.querySelectorAll('button,input:not([type="hidden"]),select,textarea')].map(element => ({ tag: element.tagName, name: element.name || element.textContent.trim(), disabled: element.disabled, width: element.getBoundingClientRect().width, height: element.getBoundingClientRect().height, display: getComputedStyle(element).display, visibility: getComputedStyle(element).visibility })),
          bootstrapNavbarBackground: getComputedStyle(document.querySelector('nav')).backgroundColor
        }));
        facts.page = item.name; facts.viewportName = viewportName; facts.httpStatus = response?.status();
        facts.passed = facts.httpStatus === 200 && facts.heading === item.heading && facts.stylesheets.length === 2 && facts.stylesheets.every(style => style.loaded) && facts.controls.every(control => !control.disabled && control.width > 0 && control.height > 0 && control.visibility === 'visible');
        summary.pages.push(facts);
        let rawScan = null;
        let accessibility = null;
        if (axeAvailability.available) {
          try {
            const scan = await scanPage(page);
            rawScan = scan;
            const file = path.join(directory, `axe-${viewportName}-${item.name}.json`);
            json(file, scan);
            accessibility = { page: item.name, viewportName, evidence: relative(file), violations: scan.violations.map(violation => ({ id: violation.id, impact: violation.impact, description: violation.description, help: violation.help, helpUrl: violation.helpUrl, nodes: violation.nodes.length, targets: violation.nodes.map(node => node.target) })), incomplete: scan.incomplete.map(rule => ({ id: rule.id, nodes: rule.nodes.length })) };
            summary.accessibility.push(accessibility);
          } catch (error) { summary.accessibility.push({ page: item.name, viewportName, error: String(error), status: 'BLOCKED' }); }
        }
        const measuredLayout = await inspectLayout(page, { directory, pageName: item.name, viewportName });
        const { evidence: rawLayout, ...layout } = measuredLayout;
        summary.layouts.push({ page: item.name, viewportName, ...layout, evidence: layout.evidencePath });
        if (item.name === 'dashboard') summary.severity.push({ viewportName, ...await severityFacts(page) });
        // Reload to restore a pristine Tab starting position after scrolling/layout work.
        await page.goto(baseUrl + item.url, { waitUntil: 'networkidle' });
        phase = `keyboard-${viewportName}-${item.name}`;
        const keyboard = await testKeyboard(page, { directory, pageName: `${viewportName}-${item.name}` });
        summary.keyboard.push({ page: item.name, viewportName, ...keyboard });
        if (accessibility) {
          accessibility.scrollableRegionVerification = await verifyScrollableRegions(page, keyboard, rawScan);
          accessibility.unresolvedViolations = accessibility.violations.filter(rule => rule.id !== 'scrollable-region-focusable' ||
            accessibility.scrollableRegionVerification.some(proof => !proof.unconfirmedScannerFlag));
          accessibility.classification = 'Raw axe JSON and violations are preserved. A scrollable-region-focusable flag is unconfirmed only when this execution proves genuine Tab, Shift+Tab and ArrowRight access for every flagged table wrapper; all other rule failures remain unresolved.';
        }
      }
    }
    await page.setViewportSize(metadata.viewportDesktop);
    phase = 'navigation';
    await page.goto(baseUrl + '/', { waitUntil: 'networkidle' });
    for (const [name, target] of [['APIs', '/Apis'], ['Defects', '/Defects'], ['Dashboard', '/']]) {
      await page.getByRole('link', { name, exact: true }).click();
      await page.waitForLoadState('networkidle');
      summary.navigation.push({ action: `click ${name}`, actualUrl: page.url(), expectedUrl: baseUrl + target, passed: page.url() === baseUrl + target });
    }
    await page.getByRole('link', { name: 'Register API', exact: true }).click();
    summary.navigation.push({ action: 'click Register API', actualUrl: page.url(), passed: page.url() === baseUrl + '/Apis/Create' });
    for (const item of cases.filter(item => ['orders', 'catalogue'].includes(item.name))) {
      await page.goto(baseUrl + '/Apis', { waitUntil: 'networkidle' });
      await page.locator(`a[href="${item.url}"]`).click();
      summary.navigation.push({ action: `click Open ${item.name}`, actualUrl: page.url(), passed: page.url() === baseUrl + item.url });
    }
    for (const kind of ['results', 'defects']) {
      phase = `download-${kind}`;
      await page.goto(baseUrl + (kind === 'results' ? '/' : '/Defects'), { waitUntil: 'networkidle' });
      const [download] = await Promise.all([
        page.waitForEvent('download'),
        page.getByRole('link', { name: kind === 'results' ? 'Export Results' : 'Export CSV', exact: true }).click()
      ]);
      const name = download.suggestedFilename();
      const file = path.join(directory, 'downloads', path.basename(name));
      await download.saveAs(file);
      const error = await download.failure();
      const bytes = fs.readFileSync(file);
      summary.downloads.push({ kind, filename: name, path: relative(file), eventAtUtc: new Date().toISOString(), sourceUrl: download.url(), failure: error, bytes: bytes.length, sha256: crypto.createHash('sha256').update(bytes).digest('hex'), passed: !error && bytes.length > 0 && new RegExp(`^${kind === 'results' ? 'api-check-results' : 'defects'}-\\d{8}-\\d{6}\\.csv$`).test(name) });
    }
    const importantResourceFailures = events.filter(event => event.type === 'requestfailed' && !event.url.includes('/Reports/') || event.type === 'response' && event.status >= 400 && !event.url.endsWith('/favicon.ico') || event.type === 'pageerror');
    summary.results = [
      result(configuration.name, 1, summary.pages.every(item => item.passed) && summary.navigation.every(item => item.passed) && !importantResourceFailures.length ? 'PASS' : 'FAIL', { pages: summary.pages.length, navigation: summary.navigation, resourceFailures: importantResourceFailures }, [`${configuration.name}/browser-summary.json`, `${configuration.name}/browser-events.json`]),
      result(configuration.name, 2, summary.keyboard.every(item => item.forwardPassed) ? 'PASS' : 'FAIL', { pages: summary.keyboard.map(item => ({ page: item.page, viewportName: item.viewportName, passed: item.forwardPassed, missing: item.missing, focusFailures: item.focusFailures })) }, summary.keyboard.map(item => relative(item.evidence))),
      result(configuration.name, 3, summary.keyboard.every(item => item.reversePassed) ? 'PASS' : 'FAIL', { pages: summary.keyboard.map(item => ({ page: item.page, viewportName: item.viewportName, passed: item.reversePassed, order: item.order })) }, summary.keyboard.map(item => relative(item.evidence))),
      result(configuration.name, 4, summary.layouts.every(item => item.responsivePassed) ? 'PASS' : 'FAIL', { pages: summary.layouts.map(item => ({ page: item.page, viewportName: item.viewportName, passed: item.responsivePassed, overflowElements: item.overflowElements, overlaps: item.overlaps })) }, summary.layouts.map(item => relative(item.evidence))),
      result(configuration.name, 5, summary.severity.every(item => item.passed) && summary.layouts.every(item => !item.stretchedBadges?.length) ? 'PASS' : 'FAIL', { dashboard: summary.severity, stretchedBadges: summary.layouts.filter(item => item.stretchedBadges?.length).map(item => ({ page: item.page, viewportName: item.viewportName, badges: item.stretchedBadges })) }, [`${configuration.name}/browser-summary.json`, ...summary.layouts.filter(item => ['dashboard', 'defects'].includes(item.page)).map(item => relative(item.evidence))]),
      result(configuration.name, 6, summary.layouts.every(item => item.tablePassed) ? 'PASS' : 'FAIL', { pages: summary.layouts.filter(item => item.tables?.length).map(item => ({ page: item.page, viewportName: item.viewportName, passed: item.tablePassed, tables: item.tables })) }, summary.layouts.filter(item => ['dashboard', 'orders', 'catalogue', 'defects'].includes(item.page)).map(item => relative(item.evidence))),
      ...[7, 8].map(index => result(configuration.name, index, 'BLOCKED', 'Browser download collected; semantic CSV validation follows both browser executions.', [`${configuration.name}/browser-summary.json`]))
    ];
    summary.additionalAccessibilityStatus = summary.accessibility.some(scan => scan.unresolvedViolations?.length) || summary.keyboard.some(item => item.labelIssues.length) ? 'FAIL' : summary.accessibility.some(scan => scan.error) || !axeAvailability.available ? 'BLOCKED' : 'PASS';
    summary.limitations = ['Headless genuine browser binaries; browser chrome, operating-system dialogs, assistive-technology interactions and human usability acceptance are not exercised.', 'Axe automated rules and DOM measurements do not establish full WCAG compliance.', 'Isolated host deliberately blocks writes and scans.'];
  } catch (error) {
    summary.error = String(error.stack || error);
    log(`${configuration.name}: ERROR ${error}`);
    summary.results = titles.map((_, index) => result(configuration.name, index + 1, 'BLOCKED', `Execution stopped: ${error.message}`, [`${configuration.name}/browser-summary.json`]));
  } finally {
    if (browser) await browser.close();
    summary.completedAtUtc = new Date().toISOString();
    json(path.join(directory, 'browser-events.json'), events);
    json(path.join(directory, 'browser-summary.json'), summary);
  }
  return summary;
}

(async () => {
  const executions = [];
  // Independent browser processes and contexts; deterministic evidence ordering.
  for (const configuration of browsers) executions.push(finalizeOnly
    ? JSON.parse(fs.readFileSync(path.join(output, configuration.name, 'browser-summary.json'), 'utf8'))
    : await executeBrowser(configuration));
  if (finalizeOnly) {
    log('Finalizing saved browser executions; no browser navigation or downloads repeated.');
    fs.copyFileSync(__filename, path.join(sourceDirectory, 'Run-BrowserChecks-finalizer.cjs'));
  }
  const python = process.env.SQA20_PYTHON;
  if (python) {
    fs.copyFileSync(path.join(__dirname, 'validate_browser_csv.py'), path.join(sourceDirectory, 'validate_browser_csv.py'));
    try { execFileSync(python, [path.join(__dirname, 'validate_browser_csv.py'), output], { cwd: root, encoding: 'utf8' }); }
    catch (error) { log(`CSV validator returned ${error.status}; inspect csv-validation.json. ${error.message}`); }
  }
  const validationFile = path.join(output, 'csv-validation.json');
  const validation = fs.existsSync(validationFile) ? JSON.parse(fs.readFileSync(validationFile, 'utf8').replace(/^\uFEFF/, '')) : null;
  for (const execution of executions) {
    for (const [index, kind] of [[7, 'results'], [8, 'defects']]) {
      const record = validation?.browsers?.[execution.browser]?.[kind];
      const download = execution.downloads.find(item => item.kind === kind);
      execution.results[index - 1] = result(execution.browser, index, download?.passed ? record?.status || 'BLOCKED' : 'BLOCKED', { download: download || null, validation: record || 'No semantic validation output available.' }, ['csv-validation.json', ...(download ? [download.path] : [])]);
    }
    json(path.join(output, execution.browser, 'browser-summary.json'), execution);
  }
  const results = executions.flatMap(execution => execution.results);
  const counts = Object.fromEntries(['PASS', 'FAIL', 'BLOCKED', 'NOT RUN'].map(status => [status, results.filter(result => result.status === status).length]));
  const summary = { startedAtUtc: startedAt, completedAtUtc: new Date().toISOString(), environment: 'environment.json', executionMethod: 'AUTOMATED PLAYWRIGHT', executions: executions.map(execution => ({ browser: execution.browser, version: execution.version, evidence: `${execution.browser}/browser-summary.json`, accessibilityStatus: execution.additionalAccessibilityStatus })), results, counts, overallAutomatedStatus: counts.FAIL || executions.some(execution => execution.additionalAccessibilityStatus === 'FAIL') ? 'FAIL' : counts.BLOCKED || executions.some(execution => execution.additionalAccessibilityStatus !== 'PASS') ? 'BLOCKED' : 'PASS', humanAcceptance: 'NOT PERFORMED; no participant responses or human SQA-19 outcome inferred.' };
  json(path.join(output, 'browser-execution.json'), summary);
  log(`Completed: ${JSON.stringify(counts)}; overall automated ${summary.overallAutomatedStatus}`);
})().catch(error => { log(String(error.stack || error)); process.exitCode = 1; });
