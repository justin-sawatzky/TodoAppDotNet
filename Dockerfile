# Multi-stage Dockerfile to build both frontend and backend
FROM node:18-alpine AS frontend-build

WORKDIR /app/frontend
COPY frontend/TodoAppFrontend/package*.json ./
RUN npm ci --only=production

COPY frontend/TodoAppFrontend/ ./
RUN npm run build

# Backend build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build

WORKDIR /app/backend
COPY backend/TodoAppDotNet.csproj ./
RUN dotnet restore "TodoAppDotNet.csproj"

COPY backend/ ./
RUN dotnet publish "TodoAppDotNet.csproj" -c Release -o /app/backend/publish /p:UseAppHost=false

# Final stage - nginx serving frontend with backend
FROM nginx:alpine AS final

# Install .NET runtime for backend
RUN apk add --no-cache dotnet10-runtime

# Copy frontend build
COPY --from=frontend-build /app/frontend/dist /usr/share/nginx/html

# Copy backend
COPY --from=backend-build /app/backend/publish /app/backend

# Copy nginx configuration
COPY nginx-combined.conf /etc/nginx/conf.d/default.conf

# Copy startup script
COPY start-combined.sh /start-combined.sh
RUN chmod +x /start-combined.sh

# Expose ports
EXPOSE 80 5247

# Start both services
CMD ["/start-combined.sh"]