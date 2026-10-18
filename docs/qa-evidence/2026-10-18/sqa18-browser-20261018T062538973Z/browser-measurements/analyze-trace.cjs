// Offline evidence extraction only; never executes trace contents or contacts the app.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');
const input = path.join(__dirname, 'Trace-20261018T195043.json');
const bytes = fs.readFileSync(input);
const trace = JSON.parse(bytes.toString('utf8'));
const events = trace.traceEvents;
const url = 'http://127.0.0.1:51818/';
const navigations = events.filter(e => e.name === 'navigationStart' && e.args?.data?.documentLoaderURL === url);
assert.equal(navigations.length, 1, 'Expected exactly one dashboard navigation; do not mix samples.');
const navigation = navigations[0];
const navigationId = navigation.args.data.navigationId;
const frame = navigation.args.frame;
const nextNavigationTs = Math.min(...events.filter(e => e.name === 'navigationStart' && e.args?.frame === frame && e.ts > navigation.ts).map(e => e.ts));
const duringNavigation = e => e.ts >= navigation.ts && e.ts < nextNavigationTs;
const delta = e => Number(((e.ts - navigation.ts) / 1000).toFixed(6));
const milestones = name => events.filter(e => e.name === name && duringNavigation(e)
    && e.args?.frame === frame && (!e.args.data?.navigationId || e.args.data.navigationId === navigationId))
    .map(e => ({ Event: e.name, TimestampUs: e.ts, SinceNavigationMs: delta(e), Data: e.args.data ?? null }));
const fcp = milestones('firstContentfulPaint');
const lcp = milestones('largestContentfulPaint::Candidate');
assert.equal(fcp.length, 1);
assert.equal(lcp.length, 1);
assert.equal(fcp[0].Data.navigationId, navigationId);
assert.equal(lcp[0].Data.navigationId, navigationId);
const responses = events.filter(e => e.name === 'ResourceReceiveResponse' && duringNavigation(e) && e.args?.data?.frame === frame);
const document = responses.find(e => e.args.data.requestId === navigationId);
assert.ok(document, 'Missing target document response.');
const header = document.args.data.headers.find(h => h.name.toLowerCase() === 'x-pt01-fixture-sha256');
assert.equal(header?.value, 'CCBE6723FB603221C3CECB95BB176BD6426E8F7611B96D3AC0565A1D7927AC18');
const requests = new Map(events.filter(e => e.name === 'ResourceSendRequest').map(e => [e.args.data.requestId, e.args.data.url]));
const screenshots = events.filter(e => e.name === 'Screenshot');
const result = {
    Source: path.basename(input), SourceSHA256: crypto.createHash('sha256').update(bytes).digest('hex').toUpperCase(),
    ExtractorRuntime: process.version, EventCount: events.length,
    RecordingStartUtc: trace.metadata.startTime, SourceType: trace.metadata.source,
    Url: url, NavigationId: navigationId, Frame: frame, NavigationStartTimestampUs: navigation.ts,
    Boundary: '(matching event.ts - dashboard navigationStart.ts) / 1000; microseconds to milliseconds; initial blank navigation excluded',
    FCP: fcp, LCPCandidates: lcp, LCPFinalizationVerified: false,
    DOMContentLoadedStart: milestones('domContentLoadedEventStart'), DOMContentLoadedEnd: milestones('domContentLoadedEventEnd'),
    LoadStart: milestones('loadEventStart'), LoadEnd: milestones('loadEventEnd'),
    FixtureSHA256: header.value,
    Responses: responses.map(e => ({Url: requests.get(e.args.data.requestId) ?? null, StatusCode: e.args.data.statusCode,
        FromCache: e.args.data.fromCache, FromServiceWorker: e.args.data.fromServiceWorker})),
    TotalScreenshotEvents: screenshots.length,
    DashboardNavigationScreenshotOffsetsMs: screenshots.filter(duringNavigation).map(delta),
    HostDPR: trace.metadata.hostDPR,
    ViewportEvents: events.filter(e => e.name === 'viewport' && e.args?.data?.frameID === frame).map(e => e.args.data),
    PaintViewportEvents: events.filter(e => e.name === 'PaintTimingVisualizer::Viewport' && duringNavigation(e)).map(e => e.args.data),
    FullChromeVersion: null, CPUThrottlingSetting: null, BrowserZoom: null,
    CompleteDashboardMs: null, Acceptance: 'PENDING TEAM DEFINITION',
    Limits: ['One trace navigation; not paired to any of the five user-reported Network samples',
        'FCP and an LCP candidate are not complete-dashboard rendering time',
        'Full Chrome version, CPU throttle, exact browser zoom and per-Network-run timestamps were not supplied',
        'Viewport/screenshot evidence does not establish a complete below-fold visual timing']
};
fs.writeFileSync(path.join(__dirname, 'trace-analysis.json'), JSON.stringify(result, null, 2) + '\n');
console.log(JSON.stringify({SourceSHA256: result.SourceSHA256, FCPMs: fcp[0].SinceNavigationMs,
    LCPCandidateMs: lcp[0].SinceNavigationMs, Acceptance: result.Acceptance}));
