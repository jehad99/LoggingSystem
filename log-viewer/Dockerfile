FROM node:20-alpine

WORKDIR /app

COPY package*.json ./

RUN npm install

RUN npm install -g @angular/cli

COPY . .

RUN npm run build

CMD ["ng", "serve", "--host", "0.0.0.0", "--proxy-config", "proxy.conf.json"]