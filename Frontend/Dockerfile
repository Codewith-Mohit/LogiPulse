# Stage 1: Build the Angular application
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files and install dependencies
COPY Frontend/package*.json ./
RUN npm install

# Copy the entire frontend source code
COPY Frontend/ .

# Execute build using a standard script target
RUN npx ng build --configuration production

# Stage 2: Serve the application using an Nginx web server
FROM nginx:alpine

COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
COPY Frontend/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80