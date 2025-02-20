import http from "k6/http";
import { sleep } from "k6";
import {
  randomIntBetween,
  uuidv4,
} from "https://jslib.k6.io/k6-utils/1.4.0/index.js";

export const options = {
  vus: 200, // Simulate 10 concurrent users
  duration: "5s", // Run test for 5 seconds
};

export default function () {
  const url = "https://localhost:7014/api/players"; // Replace with your actual API URL
  // Generate unique username & random balance
  const username = `User_${uuidv4()}`; // Random UUID-based username
  const balance = randomIntBetween(10, 1000); // Random balance between 10 - 1000

  const payload = JSON.stringify({
    username: username,
    balance: balance,
    version: 1,
  });
  const params = {
    headers: {
      "Content-Type": "application/json",
    },
  };
  const response = http.post(url, payload, params);
  console.log(`Response: ${response.status} - ${response.body}`);
  sleep(1); // Simulate some wait time before sending another request
}
