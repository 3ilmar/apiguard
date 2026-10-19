'use strict';

const fs = require('node:fs/promises');
const path = require('node:path');

async function testKeyboard(page, { directory, pageName }) {
  if (!directory || !pageName) throw new TypeError('directory and pageName are required.');
  await fs.mkdir(directory, { recursive: true });
  const safeName = pageName.replace(/[^a-z0-9_-]/gi, '-');
  const evidence = path.resolve(directory, `keyboard-${safeName}.json`);
  const startedUtc = new Date().toISOString();
  const screenshots = [];
  const errors = [];
  const forward = [];
  const reverse = [];
  let baseline = null;
  let forwardCycle = false;
  let reverseCycle = false;
  let forwardStop = '';
  let reverseStop = '';

  try {
    baseline = await page.evaluate(() => {
      const all = Array.from(document.querySelectorAll('*'));
      const normalize = value => (value || '').replace(/\s+/g, ' ').trim();
      const keyOf = element => element && element !== document.body && element !== document.documentElement
        ? `dom-${all.indexOf(element)}` : null;
      const styleOf = element => {
        const style = getComputedStyle(element);
        return Object.fromEntries(['outlineStyle', 'outlineWidth', 'outlineColor', 'outlineOffset',
          'borderTopColor', 'borderRightColor', 'borderBottomColor', 'borderLeftColor',
          'borderTopWidth', 'borderRightWidth', 'borderBottomWidth', 'borderLeftWidth',
          'boxShadow', 'backgroundColor', 'color'].map(property => [property, style[property]]));
      };
      const describe = element => {
        if (!element) return null;
        const rect = element.getBoundingClientRect();
        const labelledBy = normalize(element.getAttribute('aria-labelledby'));
        const labelText = element.labels ? Array.from(element.labels).map(label => normalize(label.textContent)).join(' ') : '';
        const ariaLabel = normalize(element.getAttribute('aria-label'));
        const accessibleName = labelledBy
          ? normalize(labelledBy.split(' ').map(id => document.getElementById(id)?.textContent || '').join(' '))
          : ariaLabel || labelText || (element.matches('button,a,summary,[role="button"]')
            ? normalize(element.textContent) : element.matches('input[type="submit"],input[type="button"]')
              ? normalize(element.value) : normalize(element.getAttribute('title')));
        const tag = element.tagName.toLowerCase();
        const type = element.getAttribute('type') || (tag === 'input' ? 'text' : null);
        const kind = tag === 'a' ? 'link' : type === 'checkbox' ? 'checkbox'
          : tag === 'button' || ['submit', 'button', 'reset'].includes(type) ? 'button'
            : ['input', 'select', 'textarea'].includes(tag) ? tag : 'other';
        return {
          key: keyOf(element), domIndex: all.indexOf(element), id: element.id || null,
          name: element.getAttribute('name'), text: normalize(element.textContent).slice(0, 250),
          accessibleName, labelText, ariaLabel, labelledBy: labelledBy || null,
          tag, type, kind, role: element.getAttribute('role'), tabIndex: element.tabIndex,
          disabled: element.matches(':disabled'), ariaHidden: element.closest('[aria-hidden="true"]') !== null,
          rect: { x: rect.x, y: rect.y, width: rect.width, height: rect.height },
          scroll: { left: element.scrollLeft, top: element.scrollTop,
            width: element.scrollWidth, height: element.scrollHeight,
            clientWidth: element.clientWidth, clientHeight: element.clientHeight },
          style: styleOf(element)
        };
      };
      const visible = element => {
        const style = getComputedStyle(element);
        return element.getClientRects().length > 0 && style.visibility !== 'hidden' &&
          style.visibility !== 'collapse' && style.display !== 'none' && !element.closest('[inert]');
      };
      const candidates = all.filter(element => visible(element) && !element.matches(':disabled') &&
        element.matches('a[href],area[href],button,input:not([type="hidden"]),select,textarea,summary,[tabindex],[contenteditable="true"]') &&
        element.tabIndex >= 0).filter(element => {
        if (!element.matches('input[type="radio"]') || !element.name) return true;
        const group = all.filter(other => other.matches('input[type="radio"]') &&
          other.name === element.name && other.form === element.form && visible(other) && !other.matches(':disabled'));
        return (group.find(other => other.checked) || group[0]) === element;
      });
      const optional = all.filter(element => visible(element) && !candidates.includes(element) &&
        element.matches('.table-responsive') && element.scrollWidth > element.clientWidth);
      const descriptors = candidates.map(describe);
      const optionalDescriptors = optional.map(describe);
      const state = {
        events: [], describe, keyOf, styleOf, all,
        baseline: new Map([...descriptors, ...optionalDescriptors].map(item => [item.key, item.style]))
      };
      if (window.__sqa20KeyboardListener) document.removeEventListener('keydown', window.__sqa20KeyboardListener, true);
      window.__sqa20KeyboardListener = event => {
        if (event.key === 'Tab') state.events.push({
          timestampUtc: new Date().toISOString(), performanceMs: performance.now(),
          key: event.key, code: event.code, shiftKey: event.shiftKey, ctrlKey: event.ctrlKey,
          altKey: event.altKey, metaKey: event.metaKey, isTrusted: event.isTrusted,
          target: describe(event.target), activeBefore: describe(document.activeElement)
        });
      };
      document.addEventListener('keydown', window.__sqa20KeyboardListener, true);
      window.__sqa20KeyboardState = state;
      return {
        url: location.href, title: document.title, initialActive: describe(document.activeElement),
        candidates: descriptors, optional: optionalDescriptors,
        viewport: { width: innerWidth, height: innerHeight },
        expectedDom: descriptors.map(item => item.key),
        expectedTab: descriptors.slice().sort((left, right) => {
          const leftOrder = left.tabIndex > 0 ? left.tabIndex : Number.MAX_SAFE_INTEGER;
          const rightOrder = right.tabIndex > 0 ? right.tabIndex : Number.MAX_SAFE_INTEGER;
          return leftOrder - rightOrder || left.domIndex - right.domIndex;
        }).map(item => item.key)
      };
    });

    async function pressAndRecord(phase, step) {
      const priorEventCount = await page.evaluate(() => window.__sqa20KeyboardState.events.length);
      await page.keyboard.press(phase === 'forward' ? 'Tab' : 'Shift+Tab');
      // Bootstrap enables native smooth scrolling. Wait for actual focus-scroll
      // and style transitions to settle; two frames can photograph an offscreen
      // control while the browser is still moving toward it.
      const settling = await page.evaluate(() => new Promise(resolve => {
        const start = performance.now();
        let previous = '', stable = 0;
        function frame() {
          const rect = document.activeElement.getBoundingClientRect();
          const style = getComputedStyle(document.activeElement);
          const value = [scrollX, scrollY, rect.x, rect.y, style.boxShadow, style.outlineColor].join('|');
          stable = value === previous ? stable + 1 : 0;
          previous = value;
          const elapsed = performance.now() - start;
          if ((stable >= 4 && elapsed >= 180) || elapsed >= 1200) resolve({ elapsedMs: elapsed, stableFrames: stable });
          else requestAnimationFrame(frame);
        }
        requestAnimationFrame(frame);
      }));
      const fact = await page.evaluate(eventStart => {
        const state = window.__sqa20KeyboardState;
        const active = document.activeElement;
        const descriptor = state.describe(active);
        const rect = active.getBoundingClientRect();
        const centerX = Math.max(0, Math.min(innerWidth - 1, rect.left + rect.width / 2));
        const centerY = Math.max(0, Math.min(innerHeight - 1, rect.top + rect.height / 2));
        const atPoint = document.elementFromPoint(centerX, centerY);
        const style = state.styleOf(active);
        const previousStyle = state.baseline.get(descriptor.key);
        const changed = previousStyle ? Object.keys(style).filter(key => style[key] !== previousStyle[key]) : [];
        const outlineVisible = style.outlineStyle !== 'none' && parseFloat(style.outlineWidth) > 0 &&
          style.outlineColor !== 'transparent' && !/rgba\([^)]*,\s*0\)/.test(style.outlineColor);
        const focusVisible = active.matches(':focus-visible');
        const changedIndicator = changed.some(key => key.startsWith('outline')) && outlineVisible ||
          changed.includes('boxShadow') && style.boxShadow !== 'none' ||
          changed.some(key => key.startsWith('border')) || changed.includes('backgroundColor') || changed.includes('color');
        const inViewport = rect.width > 0 && rect.height > 0 && rect.bottom > 0 && rect.right > 0 &&
          rect.top < innerHeight && rect.left < innerWidth;
        return {
          timestampUtc: new Date().toISOString(), performanceMs: performance.now(),
          active: descriptor, bodyTransition: !descriptor.key, focusVisible,
          baselineStyle: previousStyle || null, focusedStyle: style, changedStyleProperties: changed,
          focusIndicatorDetected: Boolean(focusVisible && (changedIndicator || outlineVisible)),
          focusIndicatorMethod: 'Computed :focus-visible and outline/border/shadow/background/text changes; screenshots require visual review.',
          inViewport, centerHitTest: atPoint ? state.describe(atPoint) : null,
          centerHitMatches: Boolean(atPoint && (atPoint === active || active.contains(atPoint))),
          windowScroll: { x: scrollX, y: scrollY },
          keydownEvents: state.events.slice(eventStart),
          trustedEvent: state.events.slice(eventStart).filter(event => event.isTrusted).at(-1) || null
        };
      }, priorEventCount);
      fact.phase = phase;
      fact.step = step;
      fact.captureSettling = settling;
      return fact;
    }

    const savedKinds = new Set();
    async function capture(fact, reason) {
      const filename = `keyboard-${safeName}-${fact.phase}-${String(fact.step).padStart(3, '0')}-${fact.active.kind}.png`;
      const screenshotPath = path.resolve(directory, filename);
      await page.screenshot({ path: screenshotPath, fullPage: false, animations: 'disabled' });
      screenshots.push({ path: screenshotPath, phase: fact.phase, step: fact.step,
        elementKey: fact.active.key, kind: fact.active.kind, reason });
    }

    const limit = Math.min(120, baseline.candidates.length + baseline.optional.length + 8);
    const forwardKeys = [];
    for (let step = 1; step <= limit; step++) {
      const fact = await pressAndRecord('forward', step);
      forward.push(fact);
      if (fact.bodyTransition) continue;
      const key = fact.active.key;
      const focusFailure = !fact.focusIndicatorDetected || !fact.inViewport || !fact.centerHitMatches;
      const representative = ['link', 'button', 'input', 'select', 'checkbox', 'textarea'].includes(fact.active.kind) &&
        !savedKinds.has(fact.active.kind);
      if (representative || focusFailure) {
        await capture(fact, focusFailure ? 'Focus indicator or visibility concern' : 'Representative focused control');
        savedKinds.add(fact.active.kind);
      }
      if (forwardKeys.includes(key)) {
        forwardCycle = key === forwardKeys[0];
        forwardStop = forwardCycle ? 'Returned to the first focused element.' : 'Repeated an element before returning to the first.';
        break;
      }
      forwardKeys.push(key);
    }
    if (!forwardStop) forwardStop = 'Reached the bounded Tab count without completing a cycle.';

    const reverseStart = forward.at(-1)?.active.key || null;
    const reverseKeys = [];
    let reverseRepresentativeSaved = false;
    for (let step = 1; step <= limit; step++) {
      const fact = await pressAndRecord('reverse', step);
      reverse.push(fact);
      if (fact.bodyTransition) continue;
      const key = fact.active.key;
      const focusFailure = !fact.focusIndicatorDetected || !fact.inViewport || !fact.centerHitMatches;
      if (!reverseRepresentativeSaved || focusFailure) {
        await capture(fact, focusFailure ? 'Reverse focus indicator or visibility concern' : 'Representative reverse navigation');
        reverseRepresentativeSaved = true;
      }
      if (reverseKeys.includes(key)) {
        reverseStop = 'Repeated an element before returning to the reverse starting element.';
        break;
      }
      reverseKeys.push(key);
      if (reverseStart && key === reverseStart) {
        reverseCycle = true;
        reverseStop = 'Returned to the element focused at the start of reverse navigation.';
        break;
      }
    }
    if (!reverseStop) reverseStop = 'Reached the bounded Shift+Tab count without completing a cycle.';
  } catch (error) {
    errors.push({ message: error.message, stack: error.stack });
  }

  const uniqueKeys = sequence => [...new Set(sequence.filter(fact => !fact.bodyTransition).map(fact => fact.active.key))];
  const forwardObserved = uniqueKeys(forward);
  const reverseObserved = uniqueKeys(reverse);
  const mandatory = baseline?.expectedDom || [];
  const missing = mandatory.filter(key => !forwardObserved.includes(key));
  const reverseMissing = mandatory.filter(key => !reverseObserved.includes(key));
  const forwardMandatory = forwardObserved.filter(key => mandatory.includes(key));
  let expectedDom = mandatory;
  // A genuinely pre-focused control can make a logical traversal start partway through the DOM.
  if (baseline?.initialActive?.key && forwardMandatory.length) {
    const start = mandatory.indexOf(forwardMandatory[0]);
    if (start > 0) expectedDom = mandatory.slice(start).concat(mandatory.slice(0, start));
  }
  const forwardMatchesDom = JSON.stringify(forwardMandatory) === JSON.stringify(expectedDom);
  const reverseExpected = forwardObserved.length ? forwardObserved.slice(1).reverse().concat(forwardObserved[0]) : [];
  const reverseMatches = forwardCycle && JSON.stringify(reverseObserved) === JSON.stringify(reverseExpected);
  const focusFailures = [...forward, ...reverse].filter(fact => !fact.bodyTransition &&
    (!fact.focusIndicatorDetected || !fact.inViewport || !fact.centerHitMatches)).map(fact => ({
      phase: fact.phase, step: fact.step, active: fact.active,
      focusIndicatorDetected: fact.focusIndicatorDetected, inViewport: fact.inViewport,
      centerHitMatches: fact.centerHitMatches, changedStyleProperties: fact.changedStyleProperties
    }));
  const labelIssues = (baseline?.candidates || []).filter(item => !item.accessibleName || item.ariaHidden).map(item => ({
    key: item.key, tag: item.tag, type: item.type, id: item.id,
    accessibleName: item.accessibleName, issue: item.ariaHidden ? 'Focusable element within aria-hidden content' : 'No accessible name found by DOM inspection'
  }));
  let events = [];
  try { events = await page.evaluate(() => window.__sqa20KeyboardState?.events || []); }
  catch (error) { errors.push({ message: `Could not retrieve keyboard events: ${error.message}` }); }
  const trustedForwardEvents = events.filter(event => !event.shiftKey && event.isTrusted).length;
  const trustedReverseEvents = events.filter(event => event.shiftKey && event.isTrusted).length;
  const forwardPassed = errors.length === 0 && mandatory.length > 0 && forwardCycle && missing.length === 0 &&
    forwardMatchesDom && trustedForwardEvents > 0 && !focusFailures.some(fact => fact.phase === 'forward');
  const reversePassed = errors.length === 0 && mandatory.length > 0 && forwardCycle && reverseCycle &&
    reverseMissing.length === 0 && reverseMatches && trustedReverseEvents > 0 && !focusFailures.some(fact => fact.phase === 'reverse');
  const result = {
    method: 'Automated Playwright keyboard.press Tab / Shift+Tab with trusted DOM event evidence; no element.focus or synthetic dispatch.',
    startedUtc, completedUtc: new Date().toISOString(), pageName, baseline,
    forwardPassed, reversePassed, missing, reverseMissing, focusFailures, labelIssues,
    forwardCycle, reverseCycle, forwardStop, reverseStop,
    order: { expectedDom, expectedTab: baseline?.expectedTab || [], forwardObserved,
      forwardMandatory, forwardMatchesDom, reverseExpected, reverseObserved, reverseMatches },
    trustedForwardEvents, trustedReverseEvents, events, forward, reverse, screenshots, errors, evidence,
    limitations: [
      'Computed focus indicators and center hit testing are objective heuristics; screenshot inspection is needed to assess their visual clarity.',
      'Browser chrome focus transitions may leave document.activeElement as BODY and have no page keydown event.',
      'Accessible names are inspected from DOM associations; axe provides the complementary accessibility rule scan.'
    ]
  };
  await fs.writeFile(evidence, JSON.stringify(result, null, 2) + '\n', 'utf8');
  return result;
}

module.exports = { testKeyboard };
