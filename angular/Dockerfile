FROM node:20-alpine AS ng-builder
RUN mkdir -p /app
WORKDIR /app
COPY . /app
ENV NODE_OPTIONS=--max_old_space_size=8192
RUN npm ci
RUN npx ng build --configuration=production --output-hashing=all --output-path /app/dist/publish/ --base-href=/

FROM nginx
EXPOSE 80
COPY --from=ng-builder /app/dist/publish/browser /usr/share/nginx/html
CMD ["nginx", "-g", "daemon off;"]
