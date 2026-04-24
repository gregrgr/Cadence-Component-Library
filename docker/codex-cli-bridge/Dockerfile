FROM node:22-bookworm-slim

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates git \
    && rm -rf /var/lib/apt/lists/* \
    && npm install -g @openai/codex

WORKDIR /app
COPY docker/codex-cli-bridge/server.js /app/server.js

ENV CODEX_HOME=/codex-home
ENV PORT=4517

EXPOSE 4517
ENTRYPOINT ["node", "/app/server.js"]
