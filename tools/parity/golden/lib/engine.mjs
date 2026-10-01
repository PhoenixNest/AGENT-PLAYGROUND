import { existsSync } from "node:fs";

export async function runInEngine(kind, fn, arg) {
  if (kind === "node") return fn(arg);
  if (kind !== "chromium") throw new Error(`unknown engine ${kind}`);
  const { chromium } = await import("playwright");
  const browser = await chromium.launch({ args: ["--force-color-profile=srgb"] });
  try {
    const page = await browser.newPage();
    return await page.evaluate(fn, arg);
  } finally {
    await browser.close();
  }
}

export async function engineLabel(kind) {
  if (kind === "node") return `node ${process.version} (v8 ${process.versions.v8})`;
  const { chromium } = await import("playwright");
  const browser = await chromium.launch();
  try {
    return `chromium ${browser.version()}`;
  } finally {
    await browser.close();
  }
}

export function chromiumAvailable() {
  try {
    return existsSync(new URL("../../node_modules/playwright/package.json", import.meta.url));
  } catch {
    return false;
  }
}
