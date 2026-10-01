import { chromium } from 'playwright';

const browser = await chromium.launch();
const context = await browser.newContext({ viewport: { width: 430, height: 932 }, deviceScaleFactor: 3 });
const page = await context.newPage();
const cdp = await context.newCDPSession(page);
const result = { chromium: browser.version() };
try {
  await cdp.send('Emulation.setSafeAreaInsetsOverride', { insets: { top: 59, right: 0, bottom: 34, left: 0 } });
  await page.setContent('<div id="t" style="padding:env(safe-area-inset-top,0px) env(safe-area-inset-right,0px) env(safe-area-inset-bottom,0px) env(safe-area-inset-left,0px)"></div>');
  result.cdpOverride = await page.evaluate(() => {
    const s = getComputedStyle(document.getElementById('t'));
    return { top: s.paddingTop, right: s.paddingRight, bottom: s.paddingBottom, left: s.paddingLeft };
  });
} catch (e) {
  result.cdpError = String(e.message).split('\n')[0];
}
console.log(JSON.stringify(result, null, 2));
await browser.close();
