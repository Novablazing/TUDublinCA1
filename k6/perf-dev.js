import { browser } from 'k6/browser';
import http from 'k6/http';
import { check, sleep } from 'k6';

// ---------------- Configuration ----------------
const BASE_URL  = __ENV.BASE_URL  || 'https://dev-nonprod01.azurewebsites.net'; // <- set your host
const CALC_PATH = __ENV.CALC_PATH || '/';        // page that renders the calculator form
const POST_PATH = __ENV.POST_PATH || CALC_PATH;  // form action; override if different

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
    ui: {
      executor: 'shared-iterations',
      exec: 'uiScenario',
      vus: 1,
      iterations: 1,
      options: { browser: { type: 'chromium' } },
    },
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

    // Adjust selectors if your markup differs
    const systolicInput  = page.locator('input#Systolic, input[name="Systolic"]');
    const diastolicInput = page.locator('input#Diastolic, input[name="Diastolic"]');
    const submitButton   = page.locator('button:has-text("Submit"), input[type="submit"]');

    await systolicInput.fill(String(SBP));
    await diastolicInput.fill(String(DBP));
    await submitButton.click();

    // Let the UI finish rendering
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(300);

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

// ---------------- Scenario 2: Protocol (HTTP) ----------------
export function apiScenario() {
  // 1) GET form page to obtain Anti-Forgery cookie + hidden token
  const getRes = http.get(`${BASE_URL}${CALC_PATH}`);
  check(getRes, { 'GET form page 200': (r) => r.status === 200 });

  // Extract hidden __RequestVerificationToken from HTML (supports ' and ")
  const tokenRegex = /<input[^>]*name=["']__RequestVerificationToken["'][^>]*value=["']([^"']+)["']/i;
  const match = getRes.body.match(tokenRegex);
  const antiToken = match ? match[1] : null;

  check(antiToken, { 'Anti-Forgery token found': (t) => !!t });

  // 2) POST the form — passing a plain object => auto x-www-form-urlencoded
  const form = {
    '__RequestVerificationToken': antiToken,
    'Systolic':  String(SBP),
    'Diastolic': String(DBP),
  };

  const postRes = http.post(`${BASE_URL}${POST_PATH}`, form);
  check(postRes, {
    'POST is OK or Redirect': (r) => r && [200, 302].includes(r.status),
  });

  // After redirect (k6 follows by default), validate content
  const body = postRes.body || '';
  check(body, {
    'Body: Category present':               (b) => b.includes('Category'),
    'Body: MAP label present':              (b) => b.includes('Mean Arterial Pressure'),
    [`Body: MAP shows ${EXPECTED_MAP_1DP}`]: (b) => b.includes(EXPECTED_MAP_1DP),
  });

  sleep(1);
}