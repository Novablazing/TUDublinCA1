
import { browser } from 'k6/browser';
import http from 'k6/http';
import { check, sleep } from 'k6';

// ---------------- Configuration ----------------
const BASE_URL  = __ENV.BASE_URL  || 'https://qa-nonprod01.azurewebsites.net'; // set to your host
const CALC_PATH = __ENV.CALC_PATH || '/';        // path for the calculator page
const POST_PATH = __ENV.POST_PATH || CALC_PATH;  // form's post target (often identical)

const SBP = Number(__ENV.SBP || 100);
const DBP = Number(__ENV.DBP || 60);

// Expected MAP: (SBP + 2*DBP)/3
function expectedMAP(sbp, dbp) {
  return ((sbp + 2 * dbp) / 3);
}
const EXPECTED_MAP = expectedMAP(SBP, DBP);
const EXPECTED_MAP_1DP = (Math.round(EXPECTED_MAP * 10) / 10).toFixed(1);

// ---------------- Scenarios ----------------
export const options = {
  scenarios: {
    // UI/E2E using real Chromium via k6/browser
    ui: {
      executor: 'shared-iterations',
      exec: 'uiScenario',
      vus: 1,
      iterations: 1,
      options: { browser: { type: 'chromium' } },
    },

    // Protocol-level (HTTP) load
    api: {
      executor: 'per-vu-iterations',
      exec: 'apiScenario',
      vus: Number(__ENV.VUS || 5),
      iterations: Number(__ENV.ITER || 5),
      maxDuration: '2m',
    },
  },
  thresholds: {
    checks: ['rate==1.0'],
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<1000'],
  },
};

// ---------------- Scenario 1: Browser (E2E) ----------------
export async function uiScenario() {
  const page = await browser.newPage();
  try {
    await page.goto(`${BASE_URL}${CALC_PATH}`, { waitUntil: 'networkidle' });

    // Try common IDs/names first; adjust if your markup differs
    const systolicInput  = page.locator('input#Systolic, input[name="Systolic"]');
    const diastolicInput = page.locator('input#Diastolic, input[name="Diastolic"]');
    const submitButton   = page.locator('button:has-text("Submit"), input[type="submit"]');

    await systolicInput.fill(String(SBP));
    await diastolicInput.fill(String(DBP));
    await submitButton.click();

    // Wait for the calculation to complete and the UI to settle
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(300); // small buffer for UI rendering

    const html = await page.content();

    check(html, {
      'UI: Category label visible':                (t) => t.includes('Category'),
      'UI: MAP label visible':                     (t) => t.includes('Mean Arterial Pressure'),
      [`UI: MAP displays ${EXPECTED_MAP_1DP}`]:    (t) => t.includes(EXPECTED_MAP_1DP),
      'UI: Summary present':                       (t) => t.includes('Summary'),
    });
  } finally {
    await page.close();
  }
  sleep(1);
}

