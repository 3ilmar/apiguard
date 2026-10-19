const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const read = name => JSON.parse(fs.readFileSync(path.join(__dirname, name), 'utf8').replace(/^\uFEFF/, ''));
const report = read('results.json');
const review = read('visual-review.json');
assert.equal(report.runs.length, 5);
assert.equal(new Set(report.runs.map(r => r.ready.timeOrigin)).size, 5);
assert.equal(report.fixtureBefore.resultCount, 2000);
assert.equal(report.fixtureBefore.defectCount, 80);
assert.deepEqual(report.fixtureBefore, report.fixtureAfter);
assert.equal(report.numericAndDOMChecksPass, true);
const traceChecks = report.runs.map(run => {
    const events = read(`run-${run.run}/performance-trace.json`).traceEvents;
    const end = events.filter(e => e.name === 'sqa18-screenshot-verified');
    assert.equal(end.length, 1);
    const navigationId = end[0].args.data.navigationId;
    const navigation = events.find(e => e.name === 'navigationStart' && e.args?.data?.navigationId === navigationId);
    assert.equal(navigation.args.data.documentLoaderURL, 'http://127.0.0.1:51818/');
    const bound = run.visualReadinessUpperBoundMs;
    const traceMs = (end[0].ts - navigation.ts) / 1000;
    assert.ok(bound < 2000 && bound > run.ready.layoutReadyMs);
    assert.ok(Math.abs(traceMs - bound) < 1, 'Trace clock differs from browser sample by >=1ms');
    assert.equal(run.ready.navigation.type, 'reload');
    assert.deepEqual(run.ready.viewport, { width: 1440, height: 1600, dpr: 1 });
    assert.deepEqual(run.ready.errors, []);
    assert.equal(run.ready.recentChecks, 12); assert.equal(run.ready.recentDefects, 8);
    assert.equal(run.ready.fontsStatus, 'loaded');
    assert.equal(run.responses.length, 3); assert.ok(run.responses.every(r => r.status === 200));
    assert.deepEqual(run.requestFailures, []); assert.deepEqual(run.pageErrors, []);
    const paints = events.filter(e => e.name === 'Paint' && e.ts >= navigation.ts && e.ts < end[0].ts);
    const frames = events.filter(e => e.name === 'Screenshot' && e.ts >= navigation.ts && e.ts < end[0].ts);
    assert.ok(paints.length > 0 && frames.length > 0);
    const pixels = fs.readFileSync(path.join(__dirname, `run-${run.run}/dashboard.png`));
    assert.equal(pixels.readUInt32BE(16), 1440); assert.equal(pixels.readUInt32BE(20), 1600);
    assert.equal(review.Runs.find(r => r.Run === run.run).Result, 'PASS');
    return { run: run.run, navigationId, navigationTimestampUs: navigation.ts, verifiedMarkTimestampUs: end[0].ts,
        traceBoundMs: traceMs, recordedBoundMs: bound, priorPaintEvents: paints.length, priorFilmstripFrames: frames.length,
        screenshotSHA256: crypto.createHash('sha256').update(pixels).digest('hex').toUpperCase() };
});
const samples = report.runs.map(r => r.visualReadinessUpperBoundMs);
const sorted = [...samples].sort((a, b) => a - b);
assert.equal(sorted[2], report.visualReadiness.medianMs);
assert.equal(sorted[4], report.visualReadiness.maximumMs);
const decision = { case: 'SQA-18', decision: 'PASS', scope: 'User-proposed operational acceptance criterion v1 only',
    formallyApprovedByTeam: false, formalRequirementStatus: 'Provisional/TBC; unchanged', releaseDecision: 'PENDING',
    checkedAtUtc: new Date().toISOString(), criterion: 'PROPOSED-CRITERION.md', evidence: 'results.json', screenshotReview: 'visual-review.json',
    boundary: report.boundary, samplesMs: samples, medianMs: sorted[2], maximumMs: sorted[4], thresholdMs: 2000,
    everySampleStrictlyUnderThreshold: samples.every(ms => ms < 2000), fixtureUnchanged: true, traceChecks };
fs.writeFileSync(path.join(__dirname, 'decision.json'), JSON.stringify(decision, null, 2) + '\n');
console.log(JSON.stringify({ decision: decision.decision, formallyApprovedByTeam: false, medianMs: sorted[2], maximumMs: sorted[4] }));
