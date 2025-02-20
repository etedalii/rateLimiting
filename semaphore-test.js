import http from 'k6/http';
import { sleep } from 'k6';
import { check } from 'k6';

export const options = {
  vus: 50, // 50 concurrent virtual users
  duration: '10s', // run test for 10 seconds
};

export default function () {
  const url = 'https://localhost:7014/api/players/semaphore'; // Update with your API URL and port if necessary
  const res = http.get(url);
  
  // Check if the response status is OK
  check(res, {
    'status is 200': (r) => r.status === 200,
  });
  
  console.log(`Response: ${res.status} - ${res.body}`);
  sleep(0.5); // Sleep for half a second between iterations
}