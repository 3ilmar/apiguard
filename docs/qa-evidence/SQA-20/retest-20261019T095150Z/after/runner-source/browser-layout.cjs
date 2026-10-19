'use strict';

// Read-only layout measurements against the genuine application DOM. The only
// page state changed is scrolling; no form is submitted and no data is changed.
const fs = require('node:fs/promises');
const path = require('node:path');

async function inspectLayout(page, { directory, pageName, viewportName }) {
  if (!page || typeof page.evaluate !== 'function' || typeof page.screenshot !== 'function') {
    throw new TypeError('inspectLayout requires a Playwright Page.');
  }
  if (!directory || !pageName || !viewportName) throw new TypeError('Evidence directory, pageName and viewportName are required.');
  const safeName = value => String(value).replace(/[^a-zA-Z0-9_-]/g, '-');
  const prefix = `${safeName(viewportName)}-${safeName(pageName)}`;
  await fs.mkdir(directory, { recursive: true });
  await page.evaluate(async () => {
    if (document.fonts && document.fonts.ready) await document.fonts.ready;
    window.scrollTo(0, 0);
    document.querySelectorAll('.table-responsive').forEach(node => { node.scrollLeft = 0; });
    await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
  });

  const evidence = await page.evaluate(() => {
    const rounded = number => Math.round(number * 100) / 100;
    const rectOf = element => {
      const rect = element.getBoundingClientRect();
      return Object.fromEntries(['x', 'y', 'left', 'top', 'right', 'bottom', 'width', 'height'].map(key => [key, rounded(rect[key])]));
    };
    const rendered = element => {
      const style = getComputedStyle(element);
      return element.getClientRects().length > 0 && style.visibility !== 'hidden' && style.display !== 'none' && style.opacity !== '0';
    };
    const textOf = element => (element.innerText || element.textContent || '').replace(/\s+/g, ' ').trim();
    const nameOf = element => {
      const labelledBy = (element.getAttribute('aria-labelledby') || '').split(/\s+/).filter(Boolean)
        .map(id => document.getElementById(id)).filter(Boolean).map(textOf).join(' ').trim();
      if (labelledBy) return { value: labelledBy, source: 'aria-labelledby' };
      if ((element.getAttribute('aria-label') || '').trim()) return { value: element.getAttribute('aria-label').trim(), source: 'aria-label' };
      const nativeLabels = element.labels ? Array.from(element.labels).map(textOf).filter(Boolean).join(' ') : '';
      if (nativeLabels) return { value: nativeLabels, source: 'associated label' };
      if (element.tagName === 'BUTTON' || element.tagName === 'A' || element.getAttribute('role') === 'button') {
        const content = textOf(element);
        if (content) return { value: content, source: 'element text' };
      }
      if (element.tagName === 'INPUT' && ['button', 'submit', 'reset'].includes(element.type) && element.value) {
        return { value: element.value, source: 'input value' };
      }
      if (element.title) return { value: element.title, source: 'title' };
      return { value: '', source: 'none; placeholder and adjacent unassociated text do not provide a reliable name' };
    };
    const describe = element => {
      const style = getComputedStyle(element);
      const rect = rectOf(element);
      const center = { x: (rect.left + rect.right) / 2, y: (rect.top + rect.bottom) / 2 };
      const inViewport = center.x >= 0 && center.x < innerWidth && center.y >= 0 && center.y < innerHeight;
      const hit = inViewport ? document.elementFromPoint(center.x, center.y) : null;
      return {
        tag: element.tagName.toLowerCase(), id: element.id || null,
        name: element.getAttribute('name'), type: element.getAttribute('type'),
        text: textOf(element), accessibleName: nameOf(element),
        href: element.getAttribute('href'), role: element.getAttribute('role'),
        rect, rendered: rendered(element),
        computed: {
          fontSize: style.fontSize, lineHeight: style.lineHeight, fontWeight: style.fontWeight,
          color: style.color, backgroundColor: style.backgroundColor,
          overflowX: style.overflowX, overflowY: style.overflowY,
          whiteSpace: style.whiteSpace, overflowWrap: style.overflowWrap,
          textOverflow: style.textOverflow
        },
        hitTest: {
          point: { x: rounded(center.x), y: rounded(center.y) },
          applicable: inViewport,
          receivesPointer: inViewport ? (hit === element || element.contains(hit)) : null,
          topElement: hit ? { tag: hit.tagName.toLowerCase(), id: hit.id || null, text: textOf(hit).slice(0, 120) } : null
        }
      };
    };
    const controls = Array.from(document.querySelectorAll('a[href],button,input:not([type="hidden"]),select,textarea,[role="button"],[tabindex]')).filter(rendered);
    const headings = Array.from(document.querySelectorAll('h1,h2,h3,h4,h5,h6')).filter(rendered);
    const contentElements = Array.from(document.querySelectorAll('h1,h2,h3,h4,h5,h6,a[href],button,input:not([type="hidden"]),select,textarea,label,code,.metric-card,.list-group-item,.badge,.severity-row')).filter(rendered);
    const documentWidth = document.documentElement.clientWidth;
    const withinControlledTable = element => {
      const wrapper = element.closest('.table-responsive');
      if (!wrapper) return false;
      const style = getComputedStyle(wrapper);
      return ['auto', 'scroll'].includes(style.overflowX) && wrapper.getBoundingClientRect().right <= documentWidth + 1;
    };
    const overflowElements = contentElements.filter(element => {
      const rect = element.getBoundingClientRect();
      return (rect.left < -1 || rect.right > documentWidth + 1) && !withinControlledTable(element);
    }).map(describe);

    // Restrict overlap detection to independent headings and controls. Parent
    // containers and their descendants necessarily intersect and are not defects.
    const overlapCandidates = [...new Set([...headings, ...controls.filter(element => element.matches('a[href],button'))])];
    const overlaps = [];
    for (let first = 0; first < overlapCandidates.length; first += 1) {
      for (let second = first + 1; second < overlapCandidates.length; second += 1) {
        const a = overlapCandidates[first]; const b = overlapCandidates[second];
        if (a.contains(b) || b.contains(a) || withinControlledTable(a) || withinControlledTable(b)) continue;
        const ar = a.getBoundingClientRect(); const br = b.getBoundingClientRect();
        const width = Math.min(ar.right, br.right) - Math.max(ar.left, br.left);
        const height = Math.min(ar.bottom, br.bottom) - Math.max(ar.top, br.top);
        if (width > 2 && height > 2) overlaps.push({ first: describe(a), second: describe(b), intersection: { width: rounded(width), height: rounded(height), area: rounded(width * height) } });
      }
    }

    const formControls = controls.filter(element => element.matches('input,select,textarea'));
    const missingInputAccessibleNames = formControls.filter(element => !nameOf(element).value).map(element => ({
      ...describe(element),
      adjacentLabelText: element.parentElement ? Array.from(element.parentElement.querySelectorAll('label')).map(textOf) : [],
      recommendation: 'Associate the visible label with a unique control id using label[for], or wrap the control in its label.'
    }));
    const tables = Array.from(document.querySelectorAll('table')).filter(rendered).map((table, index) => {
      const wrapper = table.closest('.table-responsive');
      const section = table.closest('section,.card');
      const heading = section && section.querySelector('h1,h2,h3,h4,h5,h6,.card-header');
      const rowData = Array.from(table.rows).map(row => Array.from(row.cells).map(cell => ({
        tag: cell.tagName.toLowerCase(), text: textOf(cell), scope: cell.getAttribute('scope'),
        colSpan: cell.colSpan, rowSpan: cell.rowSpan, rect: rectOf(cell),
        fontSize: getComputedStyle(cell).fontSize,
        longText: textOf(cell).length > 80,
        textWrap: getComputedStyle(cell).whiteSpace,
        indicators: Array.from(cell.querySelectorAll('.badge')).map(describe)
      })));
      return {
        index, label: table.getAttribute('aria-label') || (heading ? textOf(heading) : ''),
        caption: table.caption ? textOf(table.caption) : null,
        labelledBy: table.getAttribute('aria-labelledby'), rect: rectOf(table),
        headers: Array.from(table.querySelectorAll('th')).map(cell => ({ text: textOf(cell), scope: cell.getAttribute('scope'), rect: rectOf(cell) })),
        rowCount: table.rows.length, bodyRowCount: table.tBodies.length ? Array.from(table.tBodies).reduce((sum, body) => sum + body.rows.length, 0) : 0,
        columnCounts: Array.from(table.rows).map(row => Array.from(row.cells).reduce((sum, cell) => sum + cell.colSpan, 0)),
        rows: rowData,
        hasStructuralHeaders: table.querySelectorAll('th').length > 0,
        hasContent: rowData.some(row => row.some(cell => cell.text)),
        minimumCellFontSize: Math.min(...Array.from(table.querySelectorAll('th,td')).map(cell => parseFloat(getComputedStyle(cell).fontSize))),
        wrapper: wrapper ? { tag: wrapper.tagName.toLowerCase(), className: wrapper.className, rect: rectOf(wrapper), clientWidth: wrapper.clientWidth, scrollWidth: wrapper.scrollWidth, scrollLeft: wrapper.scrollLeft, overflowX: getComputedStyle(wrapper).overflowX, hasHorizontalOverflow: wrapper.scrollWidth > wrapper.clientWidth + 1 } : null
      };
    });
    const severityLabels = Array.from(document.querySelectorAll('.badge,.severity-row span,.list-group-item span')).filter(element => /^(Critical|High|Medium|Low)$/.test(textOf(element))).map(element => {
      const range = document.createRange();
      range.selectNodeContents(element);
      const textRect = range.getBoundingClientRect();
      const elementRect = element.getBoundingClientRect();
      const style = getComputedStyle(element);
      const normalLineHeight = style.lineHeight === 'normal' ? parseFloat(style.fontSize) * 1.2 : parseFloat(style.lineHeight);
      const textGeometricallyContained = textRect.width > 0 && textRect.height > 0 && textRect.left >= elementRect.left - 1 && textRect.right <= elementRect.right + 1 && textRect.top >= elementRect.top - 1 && textRect.bottom <= elementRect.bottom + 1;
      return {
        ...describe(element), parentHeight: rounded(element.parentElement.getBoundingClientRect().height),
        textRect: Object.fromEntries(['left', 'top', 'right', 'bottom', 'width', 'height'].map(key => [key, rounded(textRect[key])])),
        normalLineHeight: rounded(normalLineHeight),
        stretched: element.classList.contains('badge') && elementRect.height > normalLineHeight * 2,
        textGeometricallyContained,
        assessment: element.classList.contains('badge') && elementRect.height > normalLineHeight * 2
          ? 'Cosmetic stretching candidate; text geometry and screenshot require separate review, not an automatic responsive failure.'
          : 'No vertical badge stretching measured.'
      };
    });
    const clippedControls = controls.filter(element => {
      let ancestor = element.parentElement;
      const rect = element.getBoundingClientRect();
      while (ancestor && ancestor !== document.body) {
        const style = getComputedStyle(ancestor); const parentRect = ancestor.getBoundingClientRect();
        if (['hidden', 'clip'].includes(style.overflowX) && (rect.left < parentRect.left - 1 || rect.right > parentRect.right + 1)) return true;
        ancestor = ancestor.parentElement;
      }
      return false;
    }).map(describe);
    return {
      capturedUtc: new Date().toISOString(), url: location.href,
      viewport: { width: innerWidth, height: innerHeight, devicePixelRatio },
      document: { clientWidth: document.documentElement.clientWidth, scrollWidth: document.documentElement.scrollWidth, clientHeight: document.documentElement.clientHeight, scrollHeight: document.documentElement.scrollHeight, bodyClientWidth: document.body.clientWidth, bodyScrollWidth: document.body.scrollWidth, horizontalOverflow: document.documentElement.scrollWidth > document.documentElement.clientWidth + 1 },
      headings: headings.map(describe), navigation: Array.from(document.querySelectorAll('nav')).map(describe),
      controls: controls.map(describe), formControls: formControls.map(describe),
      missingInputAccessibleNames, overflowElements, overlaps, clippedControls,
      severityLabels, stretchedBadges: severityLabels.filter(item => item.stretched),
      severityProgressBars: Array.from(document.querySelectorAll('[role="progressbar"]')).map(element => ({ ...describe(element), valueNow: element.getAttribute('aria-valuenow'), valueMin: element.getAttribute('aria-valuemin'), valueMax: element.getAttribute('aria-valuemax') })),
      tables,
      limitations: [
        'DOM and geometry measurements test technical criteria; they cannot independently establish visual readability, user acceptance, or full WCAG conformance.',
        'Out-of-viewport content inside a horizontally scrollable table is assessed by actual wheel scrolling rather than classified as document overflow.',
        'The absence of an HTML caption is recorded; a nearby heading may identify a table visually, while axe independently checks supported accessibility rules.',
        'The Defect Register and Recent defects are card/list structures, not HTML tables; their text, form controls and severity labels are recorded separately.'
      ]
    };
  });

  const screenshots = [];
  const fullPath = path.join(directory, `${prefix}-full.png`);
  const viewportPath = path.join(directory, `${prefix}-viewport.png`);
  await page.screenshot({ path: fullPath, fullPage: true, animations: 'disabled' });
  screenshots.push(fullPath);
  await page.screenshot({ path: viewportPath, fullPage: false, animations: 'disabled' });
  screenshots.push(viewportPath);

  for (const table of evidence.tables) {
    if (!table.wrapper || !table.wrapper.hasHorizontalOverflow) {
      table.horizontalScroll = { required: false, method: null, scrollWorks: null };
      continue;
    }
    const tableLocator = page.locator('table').nth(table.index);
    const wrapperLocator = tableLocator.locator('xpath=ancestor::*[contains(concat(" ",normalize-space(@class)," ")," table-responsive ")][1]');
    await wrapperLocator.scrollIntoViewIfNeeded();
    await wrapperLocator.hover();
    const before = await wrapperLocator.evaluate(element => ({ scrollLeft: element.scrollLeft, clientWidth: element.clientWidth, scrollWidth: element.scrollWidth }));
    // Browser input, not setting scrollLeft, is the tested operation.
    const deltaX = Math.max(500, Math.min(8000, before.scrollWidth + 200));
    await page.mouse.wheel(deltaX, 0);
    await page.waitForTimeout(180);
    const after = await wrapperLocator.evaluate(element => ({ scrollLeft: element.scrollLeft, clientWidth: element.clientWidth, scrollWidth: element.scrollWidth }));
    const rightEdgePath = path.join(directory, `${prefix}-table-${table.index + 1}-rightedge.png`);
    await page.screenshot({ path: rightEdgePath, fullPage: false, animations: 'disabled' });
    screenshots.push(rightEdgePath);
    table.horizontalScroll = {
      required: true, method: 'Playwright locator.scrollIntoViewIfNeeded + hover + genuine page.mouse.wheel(deltaX, 0)',
      deltaX, before, after, scrollWorks: after.scrollLeft > before.scrollLeft + 1,
      reachedRightEdge: after.scrollLeft + after.clientWidth >= after.scrollWidth - 2,
      screenshot: rightEdgePath
    };
    // Restore only after collecting actual browser-input evidence.
    await wrapperLocator.evaluate(element => { element.scrollLeft = 0; });
  }
  await page.evaluate(() => { window.scrollTo(0, 0); });

  const responsivePassed = !evidence.document.horizontalOverflow && evidence.overflowElements.length === 0 && evidence.overlaps.length === 0 && evidence.clippedControls.length === 0;
  const tablePassed = evidence.tables.every(table => table.hasStructuralHeaders && table.hasContent && (!table.wrapper || !table.wrapper.hasHorizontalOverflow || (table.horizontalScroll && table.horizontalScroll.scrollWorks && table.horizontalScroll.reachedRightEdge)));
  evidence.assessment = {
    responsivePassed, tablePassed,
    meaning: 'PASS refers only to the stated automated geometry, structural and native scrolling criteria. Accessibility scan and screenshot observations are assessed separately.',
    cosmeticStretchingCount: evidence.stretchedBadges.length,
    missingInputAccessibleNameCount: evidence.missingInputAccessibleNames.length
  };
  evidence.screenshots = screenshots;
  const evidencePath = path.join(directory, `layout-${prefix}.json`);
  await fs.writeFile(evidencePath, `${JSON.stringify(evidence, null, 2)}\n`, 'utf8');
  return {
    responsivePassed, tablePassed,
    overflowElements: evidence.overflowElements, overlaps: evidence.overlaps,
    stretchedBadges: evidence.stretchedBadges, tables: evidence.tables,
    missingInputAccessibleNames: evidence.missingInputAccessibleNames,
    screenshots, evidence, evidencePath
  };
}

module.exports = { inspectLayout };
