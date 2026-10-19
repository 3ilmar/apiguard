// Local SQA-18 evidence collector. Does not modify the Web app or its data.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');
function option(name) { const i = process.argv.indexOf(name); assert.ok(i > 0 && process.argv[i + 1], `Missing ${name}`); return process.argv[i + 1]; }
const output = path.resolve(option('--evidence-directory'));
const modulePath = option('--playwright-module');
const chromePath = option('--chrome');
const { chromium } = require(modulePath);
const playwrightVersion = require(path.join(modulePath, 'package.json')).version;
const url = 'http://127.0.0.1:51818/';
const expectedHash = 'CCBE6723FB603221C3CECB95BB176BD6426E8F7611B96D3AC0565A1D7927AC18';
const viewport = { width: 1440, height: 1600 };
const sha = bytes => crypto.createHash('sha256').update(bytes).digest('hex').toUpperCase();
const save = (name, object) => fs.writeFileSync(path.join(output, name), JSON.stringify(object, null, 2) + '\n');

// Executed in the browser after each actual reload; performance.now() is relative
// to that document's navigation time origin. No wall-clock subtraction is used.
async function dashboardReady() {
    const frame = () => new Promise(resolve => requestAnimationFrame(resolve));
    if (document.readyState !== 'complete') await new Promise(resolve => addEventListener('load', resolve, { once: true }));
    await document.fonts.ready;
    for (const image of document.images) { if (!image.complete) await image.decode(); }
    const expected = {
        'Registered APIs': '1', 'Configured endpoints': '1', 'Executed checks': '2000',
        'Pass rate': '50%', 'Passed / Failed': '1000 / 1000', 'Avg response': '149.5 ms',
        'Open defects': '48', 'High/Critical open': '24'
    };
    function inspect() {
        const errors = [];
        const metrics = Object.fromEntries([...document.querySelectorAll('.metric-card')].map(e => [e.querySelector('span').textContent.trim(), e.querySelector('strong').textContent.trim()]));
        for (const [label, value] of Object.entries(expected)) if (metrics[label] !== value) errors.push(`Metric ${label}: ${metrics[label]}`);
        if (Object.keys(metrics).length !== 8) errors.push('Expected eight cards');
        const links = [...document.querySelectorAll('link[rel=stylesheet]')];
        if (links.length !== 2 || links.some(e => !e.sheet || e.disabled)) errors.push('Both CSSStyleSheets must be available');
        if (getComputedStyle(document.body).backgroundColor !== 'rgb(246, 248, 251)') errors.push('Site background missing');
        const cards = [...document.querySelectorAll('.metric-card')];
        if (cards.some(e => getComputedStyle(e).display !== 'flex' || parseFloat(getComputedStyle(e).minHeight) < 100)) errors.push('Site card style missing');
        if (getComputedStyle(document.querySelector('.row')).display !== 'flex') errors.push('Bootstrap row style missing');
        const severities = [...document.querySelectorAll('.severity-row')].map(e => {
            const bar = e.querySelector('.progress-bar'); const progress = e.querySelector('.progress');
            const ratio = bar.getBoundingClientRect().width / progress.getBoundingClientRect().width;
            const record = { label: e.querySelector('span').textContent.trim(), count: e.querySelector('strong').textContent.trim(),
                now: progress.getAttribute('aria-valuenow'), max: progress.getAttribute('aria-valuemax'), fraction: ratio, colour: getComputedStyle(bar).backgroundColor };
            if (record.count !== '12' || record.now !== '12' || record.max !== '48' || Math.abs(ratio - 0.25) > 0.005
                || getComputedStyle(progress).display !== 'flex' || progress.getBoundingClientRect().height < 11) errors.push('Invalid severity bar ' + record.label);
            return record;
        });
        if (severities.map(e => e.label).join(',') !== 'Critical,High,Medium,Low') errors.push('Incorrect severity labels');
        const checks = [...document.querySelectorAll('[aria-labelledby=recent-checks-title] tbody tr')];
        const defects = [...document.querySelectorAll('[aria-labelledby=recent-defects-title] .list-group-item')];
        if (checks.length !== 12 || defects.length !== 8) errors.push('Incorrect recent-list counts');
        if (!checks[0]?.textContent.includes('PT-01 check 1999') || !defects[0]?.textContent.includes('PT-01 defect 0079')) errors.push('Incorrect newest fixture records');
        const elements = [...cards, ...document.querySelectorAll('.severity-row'), ...checks, ...defects];
        const boxes = elements.map(e => { const r = e.getBoundingClientRect(); return { x: r.x, y: r.y, w: r.width, h: r.height }; });
        for (let i = 0; i < elements.length; i++) {
            const style = getComputedStyle(elements[i]); const r = boxes[i];
            if (style.visibility !== 'visible' || style.display === 'none' || Number(style.opacity) === 0 || r.w <= 0 || r.h <= 0
                || r.x < 0 || r.y < 0 || r.x + r.w > innerWidth + 1 || r.y + r.h > innerHeight + 1) errors.push(`Element ${i} not fully in fixed viewport`);
        }
        if (document.documentElement.scrollWidth > innerWidth || document.documentElement.scrollHeight > innerHeight) errors.push('Dashboard overflows fixed viewport');
        return { errors, metrics, severities, recentChecks: checks.length, recentDefects: defects.length, boxes,
            documentHeight: document.documentElement.scrollHeight, documentWidth: document.documentElement.scrollWidth };
    }
    let previous; let stable = 0; let snapshot;
    // Two equal successive frame geometries require at least three rAF callbacks.
    for (let attempt = 0; attempt < 30; attempt++) {
        await frame(); snapshot = inspect();
        const geometry = JSON.stringify(snapshot.boxes);
        stable = geometry === previous ? stable + 1 : 0; previous = geometry;
        if (!snapshot.errors.length && stable >= 2) break;
    }
    if (snapshot.errors.length || stable < 2) throw new Error(JSON.stringify({ readinessErrors: snapshot.errors, stableFrames: stable }));
    performance.mark('sqa18-layout-ready');
    return { ...snapshot, layoutReadyMs: performance.now(), timeOrigin: performance.timeOrigin,
        navigation: performance.getEntriesByType('navigation')[0].toJSON(), stylesheets: [...document.querySelectorAll('link[rel=stylesheet]')].map(e => e.href),
        fontsStatus: document.fonts.status, visibility: document.visibilityState, viewport: { width: innerWidth, height: innerHeight, dpr: devicePixelRatio },
        userAgent: navigator.userAgent, stableFrames: stable };
}

async function fetchFixture() {
    const document = await fetch(url, { redirect: 'manual' });
    assert.equal(document.status, 200); assert.equal(document.headers.get('x-pt01-fixture-sha256'), expectedHash);
    const results = await (await fetch(url + 'Reports/ExportResultsCsv')).text();
    const defects = await (await fetch(url + 'Reports/ExportDefectsCsv')).text();
    // Existing fixture texts contain no embedded CR/LF. Header is not a result.
    const resultRows = results.trim().split(/\r?\n/); const defectRows = defects.trim().split(/\r?\n/);
    assert.equal(resultRows.length - 1, 2000); assert.equal(defectRows.length - 1, 80);
    return { resultCount: resultRows.length - 1, defectCount: defectRows.length - 1,
        fixtureSHA256: expectedHash, resultsCsvSHA256: sha(results), defectsCsvSHA256: sha(defects) };
}

(async () => {
    fs.mkdirSync(output, { recursive: true });
    assert.ok(!fs.existsSync(path.join(output, 'results.json')), 'Use a new output directory; never overwrite a run.');
    const report = { case: 'SQA-18', criterion: 'User-proposed operational criterion v1; NOT formally team-approved',
        startedUtc: new Date().toISOString(), node: process.version, playwrightVersion, chromePath,
        boundary: 'Navigation time origin to browser performance.now() immediately after a viewport screenshot has been captured and returned; conservative visual-readiness upper bound including screenshot/automation overhead',
        settings: { viewport, headed: true, deviceScaleFactor: 1, locale: 'en-NZ', timezoneId: 'Pacific/Auckland',
            cacheDisabled: true, serviceWorkers: 'block', cpuThrottlingRate: 1, network: 'No throttling; latency 0; throughput unlimited',
            isolation: 'New incognito context/page for each measured reload; one separate automation Chrome process; normal user profile never used',
            server: 'Already warmed', trace: 'CDP devtools.timeline, blink.user_timing, loading, screenshots; instrumentation enabled during all five samples' },
        runs: [], status: 'RUNNING', screenshotReview: 'PENDING' };
    save('results.json', report);
    const before = await fetchFixture(); report.fixtureBefore = before;
    const browser = await chromium.launch({ executablePath: chromePath, headless: false,
        args: ['--disable-extensions', '--no-first-run', '--window-size=1500,1800'] });
    report.chromeVersion = browser.version();
    try {
        const browserSession = await browser.newBrowserCDPSession();
        report.browserProtocolVersion = await browserSession.send('Browser.getVersion');
        for (let index = 1; index <= 5; index++) {
            const directory = path.join(output, `run-${index}`); fs.mkdirSync(directory);
            const run = { run: index, startedUtc: new Date().toISOString(), status: 'RUNNING' };
            report.runs.push(run); save('results.json', report);
            const context = await browser.newContext({ viewport, deviceScaleFactor: 1, locale: 'en-NZ', timezoneId: 'Pacific/Auckland', serviceWorkers: 'block',
                recordHar: { path: path.join(directory, 'network.har'), mode: 'full', content: 'omit' } });
            const page = await context.newPage();
            const cdp = await context.newCDPSession(page);
            const responses = []; const failures = []; const consoleErrors = []; const traceEvents = [];
            let measured = false; let tracing = false;
            page.on('response', response => { if (measured) responses.push({ url: response.url(), status: response.status(), type: response.request().resourceType() }); });
            page.on('requestfailed', request => { if (measured) failures.push({ url: request.url(), failure: request.failure() }); });
            page.on('pageerror', error => { if (measured) consoleErrors.push(error.message); });
            cdp.on('Tracing.dataCollected', e => traceEvents.push(...e.value));
            try {
                await cdp.send('Network.enable'); await cdp.send('Network.setCacheDisabled', { cacheDisabled: true });
                await cdp.send('Network.setBypassServiceWorker', { bypass: true });
                await cdp.send('Network.emulateNetworkConditions', { offline: false, latency: 0, downloadThroughput: -1, uploadThroughput: -1 });
                await cdp.send('Emulation.setCPUThrottlingRate', { rate: 1 });
                await page.goto(url, { waitUntil: 'load', timeout: 15000 }); // Preparation only; not a retained timing sample.
                await cdp.send('Network.clearBrowserCache');
                await cdp.send('Tracing.start', { categories: 'devtools.timeline,blink.user_timing,loading,disabled-by-default-devtools.screenshot', transferMode: 'ReportEvents' });
                tracing = true; measured = true;
                const response = await page.reload({ waitUntil: 'load', timeout: 15000 });
                assert.equal(response.status(), 200); assert.equal(response.headers()['x-pt01-fixture-sha256'], expectedHash);
                run.documentStatus = response.status(); run.fixtureSHA256 = response.headers()['x-pt01-fixture-sha256'];
                run.ready = await page.evaluate(dashboardReady);
                assert.equal(run.ready.navigation.type, 'reload'); assert.equal(run.ready.visibility, 'visible');
                await page.screenshot({ path: path.join(directory, 'dashboard.png'), fullPage: false, animations: 'allow', timeout: 15000 });
                run.visualReadinessUpperBoundMs = await page.evaluate(() => { performance.mark('sqa18-screenshot-verified'); return performance.now(); });
                run.navigationStartUtc = new Date(run.ready.timeOrigin).toISOString();
                run.paint = await page.evaluate(() => performance.getEntriesByType('paint').map(e => e.toJSON()));
                run.postScreenshotCorrectness = await page.evaluate(() => ({ executed: document.querySelectorAll('.metric-card strong')[2].textContent.trim(),
                    checks: document.querySelectorAll('[aria-labelledby=recent-checks-title] tbody tr').length,
                    defects: document.querySelectorAll('[aria-labelledby=recent-defects-title] .list-group-item').length }));
                assert.deepEqual(run.postScreenshotCorrectness, { executed: '2000', checks: 12, defects: 8 });
                const css = responses.filter(r => r.type === 'stylesheet');
                assert.equal(css.length, 2); assert.ok(css.every(r => r.status === 200));
                assert.equal(failures.length, 0); assert.equal(consoleErrors.length, 0);
                fs.writeFileSync(path.join(directory, 'dashboard.html'), await page.content());
                run.status = run.visualReadinessUpperBoundMs < 2000 ? 'NUMERIC PASS - SCREENSHOT REVIEW PENDING' : 'FAIL';
            } catch (error) {
                run.status = 'FAIL'; run.error = error.stack;
                await page.screenshot({ path: path.join(directory, 'failure.png'), timeout: 5000 }).catch(() => {});
            } finally {
                if (tracing) {
                    const complete = new Promise(resolve => cdp.once('Tracing.tracingComplete', resolve));
                    await cdp.send('Tracing.end'); await complete;
                    fs.writeFileSync(path.join(directory, 'performance-trace.json'), JSON.stringify({ traceEvents }));
                }
                run.responses = responses; run.requestFailures = failures; run.pageErrors = consoleErrors; run.finishedUtc = new Date().toISOString();
                fs.writeFileSync(path.join(directory, 'measurement.json'), JSON.stringify(run, null, 2) + '\n');
                await context.close(); // Flush HAR; closes only this automation context.
                save('results.json', report);
            }
            console.log(JSON.stringify({ run: index, visualReadinessUpperBoundMs: run.visualReadinessUpperBoundMs ?? null, status: run.status }));
        }
        report.fixtureAfter = await fetchFixture();
        report.fixtureUnchanged = JSON.stringify(report.fixtureAfter) === JSON.stringify(before);
        const values = report.runs.map(r => r.visualReadinessUpperBoundMs).filter(Number.isFinite).sort((a, b) => a - b);
        report.visualReadiness = { samplesMs: report.runs.map(r => r.visualReadinessUpperBoundMs ?? null), medianMs: values.length === 5 ? values[2] : null,
            maximumMs: values.length ? values.at(-1) : null, thresholdMs: 2000 };
        report.numericAndDOMChecksPass = report.runs.length === 5 && report.runs.every(r => r.status === 'NUMERIC PASS - SCREENSHOT REVIEW PENDING') && report.fixtureUnchanged;
        report.status = report.numericAndDOMChecksPass ? 'SCREENSHOT REVIEW PENDING' : 'FAIL';
    } finally {
        await browser.close(); report.finishedUtc = new Date().toISOString(); save('results.json', report);
    }
    console.log(JSON.stringify({ status: report.status, ...report.visualReadiness }));
})().catch(error => { fs.writeFileSync(path.join(output, 'runner-error.txt'), error.stack); console.error(error.message); process.exitCode = 1; });
