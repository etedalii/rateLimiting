import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
   vus: 5, // concurrent users
   duration: '10s', // Run for 10 seconds
};

export default function () {
   const url = 'https://localhost:7014/api/players/test-db-rate-limit';

   const response = http.get(url);

   console.log(`Response: ${response.status} - ${response.body}`);

   sleep(0.1); // Minimal delay between requests
}