FROM python:3.9-slim

WORKDIR /app

COPY log_producer.py /app/log_producer.py

RUN pip install requests

CMD ["python", "log_producer.py"]
