import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 1,
  iterations: 1,
};

export default function () {
  const BASE_URL = 'https://dev-nonprod01.azurewebsites.net'; // your app URL
  const CALC_PATH = '/'; // adjust if needed

  // 1. GET the page
  let res = http.get(`${BASE_URL}${CALC_PATH}`);
  check(res, { 'GET page status is 200': (r) => r.status === 200 });

  // 2. POST form data (simplified, no CSRF token)
  const payload = {
    Systolic: '100',
    Diastolic: '60',
  };

  res = http.post(`${BASE_URL}${CALC_PATH}`, payload);
  check(res, {
    'POST status is 200 or 302': (r) => [200, 302].includes(r.status),
    'Response contains Category': (r) => r.body.includes('Category'),
    'Response contains MAP': (r) => r.body.includes('Mean Arterial Pressure'),
  });

  sleep(1);
}