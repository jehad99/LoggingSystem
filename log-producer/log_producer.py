import requests
import random
import time
from datetime import datetime

# Configuration
API_URL = "http://distributed-logging-api:8080/v1/logs/add"
LOG_LEVELS = ["Info", "Warning", "Error"]
MESSAGE_TEMPLATES = [
    "System started successfully.",
    "High memory usage detected.",
    "Application crashed unexpectedly.",
    "Database connection lost.",
    "User logged in.",
    "File upload failed."
]

def generate_log():
    """Generate a random log entry."""
    return {
        "Service": "LogProducer",
        "Timestamp": datetime.utcnow().isoformat(),
        "Level": random.choice(LOG_LEVELS),
        "Message": random.choice(MESSAGE_TEMPLATES)
    }

def send_log(log):
    """Send a log to the API."""
    try:
        response = requests.post(API_URL, json=log)
        if response.status_code == 200:
            print(f"Log sent: {log}")
        else:
            print(f"Failed to send log: {response.status_code} - {response.text}")
    except Exception as e:
        print(f"Error sending log: {e}")

def main():
    """Main loop to produce logs."""
    print("Starting log producer...")
    while True:
        log = generate_log()
        send_log(log)
        time.sleep(random.uniform(0.5, 2.0))  # Simulate random log intervals

if __name__ == "__main__":
    main()
