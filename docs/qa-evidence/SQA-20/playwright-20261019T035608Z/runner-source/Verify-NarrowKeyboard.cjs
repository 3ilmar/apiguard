'use strict';
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require(require.resolve('playwright', { paths: [process.env.SQA20_NODE_MODULES || __dirname] }));
const { testKeyboard } = require('./browser-keyboard.cjs');
const output = path.resolve(process.argv[2] || '');
if (!process.argv[2]) throw new Error('Supply existing session path.');
const evidence = path.join(output, 'narrow-keyboard-verification.json');
if (fs.existsSync(evidence)) throw new Error('Refusing to overwrite prior narrow verification.');
const browsers = [
  ['chrome', 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe'],
  ['edge', 'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe']
];
(async () => {
  const results = [];
  for (const [name, executablePath] of browsers) {
    const browser = await chromium.launch({ executablePath, headless: true });
    try {
      const context = await browser.newContext({ viewport: { width: 375, height: 812 }, deviceScaleFactor: 1 });
      await context.route('**/*', route => {
        const url = route.request().url();
        return url.startsWith('http://localhost:51920/') || url === 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css' ? route.continue() : route.abort();
      });
      const page = await context.newPage();
      for (const [pageName, id] of [['orders', '20200000-0000-0000-0000-000000000001'], ['catalogue', '20200000-0000-0000-0000-000000000002']]) {
        await page.goto(`http://localhost:51920/Apis/Details/${id}`, { waitUntil: 'networkidle' });
        const keyboard = await testKeyboard(page, { directory: path.join(output, name), pageName: `narrow-${pageName}` });
        const wrappers = await page.evaluate(() => [...document.querySelectorAll('.table-responsive')].map((element, index) => ({
          index, key: `dom-${[...document.querySelectorAll('*')].indexOf(element)}`, tabIndex: element.tabIndex,
          clientWidth: element.clientWidth, scrollWidth: element.scrollWidth,
          overflow: element.scrollWidth > element.clientWidth + 1,
          attributeTabIndex: element.getAttribute('tabindex')
        })));
        for (const wrapper of wrappers.filter(wrapper => wrapper.overflow)) {
          wrapper.reachedByTab = keyboard.forward.some(event => event.active.key === wrapper.key);
          wrapper.reachedByShiftTab = keyboard.reverse.some(event => event.active.key === wrapper.key);
          if (wrapper.reachedByTab) {
            for (let step = 0; step < 120; step++) {
              await page.keyboard.press('Tab');
              const current = await page.evaluate(() => `dom-${[...document.querySelectorAll('*')].indexOf(document.activeElement)}`);
              if (current === wrapper.key) break;
            }
            wrapper.scrollBefore = await page.locator('.table-responsive').nth(wrapper.index).evaluate(element => element.scrollLeft);
            await page.keyboard.press('ArrowRight');
            await page.waitForTimeout(180);
            wrapper.scrollAfter = await page.locator('.table-responsive').nth(wrapper.index).evaluate(element => element.scrollLeft);
            wrapper.keyboardScrollWorks = wrapper.scrollAfter > wrapper.scrollBefore;
          } else wrapper.keyboardScrollWorks = false;
        }
        results.push({ browser: name, version: browser.version(), page: pageName, viewport: { width: 375, height: 812 }, atUtc: new Date().toISOString(), forwardPassed: keyboard.forwardPassed, reversePassed: keyboard.reversePassed, keyboardEvidence: keyboard.evidence, wrappers });
        console.log(`${name} ${pageName}: ${JSON.stringify(wrappers.filter(wrapper => wrapper.overflow))}`);
      }
    } finally { await browser.close(); }
  }
  fs.copyFileSync(__filename, path.join(output, 'runner-source', path.basename(__filename)));
  fs.writeFileSync(evidence, JSON.stringify({ method: 'Targeted genuine Tab/Shift+Tab/ArrowRight verification of axe scrollable-region-focusable findings; no DOM focus or tabindex mutation.', results }, null, 2) + '\n');
})().catch(error => { console.error(error.stack); process.exitCode = 1; });
