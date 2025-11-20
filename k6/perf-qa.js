import { check, sleep } from "k6";
import http from "k6/http";

// Export an options object to configure how k6 will behave during test execution.
//
// See https://docs.k6.io/docs/options
//
export let options = {

  // Either specify vus + duration or stages
  // vus: 20,
  // duration: "3m",

  // This stages configuration will ramp to 20 Virtual Users over a minute,
  // maintain those 20 concurrent users for 1 minute
  // then ramp down to 0 over a minute i.e. ramp-up pattern of "load"
  stages: [
    { duration: "1m", target: 20 },            // 1 new vu every 3 seconds
    { duration: "1m", target: 20 },
    { duration: "1m", target: 0 }             // 1 less vu every 3 seconds
  ],
  
  // set a threshold at 100 ms request duration for 95th percentile
  // request duration = time spent sending request, waiting for response, and receiving response
  // aka "response time"
  // the test will be marked as failed by threshold if the value is exceeded 
  // i.e. 95% of request duration times should be < 300 ms
 	thresholds: {
    "http_req_duration": ["p(95) < 300"]
  },

  // Don't save the bodies of HTTP responses by default, for improved performance
  // Can be overwritten by setting the `responseType` option to `text` or `binary` for individual requests
  discardResponseBodies: false,
  // overriden below for GET
  
  cloud: {
    distribution: {
      distributionLabel1: { loadZone: 'amazon:us:ashburn', percent: 50 },
      distributionLabel2: { loadZone: 'amazon:ie:dublin', percent: 50 },
    },
  },
 
};

/**
 * Get a random integer between `min` and `max`.
 * 
 * @param {number} min - min number
 * @param {number} max - max number
 * @return {number} a random integer
 */

function getRandomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1) + min);
}

// Export a default function - this defines the entry point for your VUs,
// similar to the main() function in many other languages.
export default function() {
 
  // do an initial GET, force response body so that form can be subsequently submited
  let res = http.get("https://test-nonprod.azurewebsites.net/bmi", {"responseType": "text"});

  // cookies automatically handled i.e. cookies sent by server will be re-presented by the client in all subsequent requests
  // until end of script

  check(res, {
    "is status 200": (r) => r.status === 200
  });

  // now do a POST without breaking CSRF feature of ASP.Net Core i.e. include cookie and matching hidden
  // field from result (__RequestVerificationToken)

  // now POST form with cookie set and hidden field set to anti forgery token etc. 
  // POST with random data to prevent server cached response to POST, discard response body
  res = res.submitForm({
    fields: { Bmi_WeightStones: getRandomInt(5, 50).toString(), Bmi_WeightPounds: getRandomInt(0, 13).toString(), 
              Bmi_HeightFeet: getRandomInt(4, 7).toString(), Bmi_HeightInches: getRandomInt(0, 11).toString()}
  });

  check(res, {
    "is status 200": (r) => r.status === 200
  });

  // "think" for 3 seconds
  sleep(3);
}

// to run on Docker:
// docker pull grafana/k6                                
// docker run -i grafana/k6 run - <perf2.js