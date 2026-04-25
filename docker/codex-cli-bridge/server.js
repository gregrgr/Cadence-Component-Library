const http = require("node:http");
const { spawn } = require("node:child_process");
const fs = require("node:fs/promises");
const os = require("node:os");
const path = require("node:path");

const port = Number.parseInt(process.env.PORT || "4517", 10);
const bridgeToken = process.env.CODEX_BRIDGE_TOKEN || "";
const defaultCommand = process.env.CODEX_COMMAND || "codex";

function writeJson(response, statusCode, body) {
  const json = JSON.stringify(body);
  response.writeHead(statusCode, {
    "content-type": "application/json; charset=utf-8",
    "content-length": Buffer.byteLength(json)
  });
  response.end(json);
}

function readBody(request) {
  return new Promise((resolve, reject) => {
    let body = "";
    request.setEncoding("utf8");
    request.on("data", chunk => {
      body += chunk;
      if (body.length > 2_000_000) {
        reject(new Error("Request body is too large."));
        request.destroy();
      }
    });
    request.on("end", () => resolve(body));
    request.on("error", reject);
  });
}

async function runCodex(request) {
  if (!request.prompt || typeof request.prompt !== "string") {
    throw new Error("Request body must include a prompt string.");
  }

  await assertLoggedIn(stringOrDefault(request.command, defaultCommand));

  const outputPath = path.join(os.tmpdir(), `cadence-codex-${cryptoRandomId()}.json`);
  const timeoutSeconds = clampInteger(request.timeoutSeconds, 10, 1800, 180);
  const command = stringOrDefault(request.command, defaultCommand);
  const sandbox = stringOrDefault(request.sandbox, "read-only");
  const args = [
    "exec",
    "--skip-git-repo-check",
    "--sandbox",
    sandbox,
    "--color",
    "never",
    "--output-last-message",
    outputPath
  ];

  if (request.ephemeral !== false) {
    args.push("--ephemeral");
  }

  if (request.model) {
    args.push("--model", String(request.model));
  }

  if (request.profile) {
    args.push("--profile", String(request.profile));
  }

  args.push("-");

  const child = spawn(command, args, {
    cwd: request.workingDirectory || "/workspace",
    stdio: ["pipe", "pipe", "pipe"],
    env: process.env
  });

  console.log(`Starting codex exec with sandbox=${sandbox}, timeout=${timeoutSeconds}s`);
  let stdout = "";
  let stderr = "";
  child.stdout.setEncoding("utf8");
  child.stderr.setEncoding("utf8");
  child.stdout.on("data", chunk => {
    stdout += chunk;
  });
  child.stderr.on("data", chunk => {
    stderr += chunk;
  });

  child.stdin.write(request.prompt);
  child.stdin.end();

  const exitCode = await waitForExit(child, timeoutSeconds);
  console.log(`codex exec exited with code ${exitCode}`);
  let output = stdout;
  try {
    output = await fs.readFile(outputPath, "utf8");
  } catch {
  } finally {
    await fs.rm(outputPath, { force: true }).catch(() => {});
  }

  return { exitCode, output, errorOutput: stderr };
}

async function assertLoggedIn(command) {
  const child = spawn(command, ["login", "status"], {
    stdio: ["ignore", "pipe", "pipe"],
    env: process.env
  });

  let stdout = "";
  let stderr = "";
  child.stdout.setEncoding("utf8");
  child.stderr.setEncoding("utf8");
  child.stdout.on("data", chunk => {
    stdout += chunk;
  });
  child.stderr.on("data", chunk => {
    stderr += chunk;
  });

  const exitCode = await waitForExit(child, 10);
  if (exitCode !== 0) {
    const message = (stdout || stderr || "Not logged in").trim();
    throw new Error(`Codex CLI is not logged in inside the codex-cli container. ${message}`);
  }
}

function waitForExit(child, timeoutSeconds) {
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => {
      child.kill("SIGKILL");
      reject(new Error(`Codex CLI timed out after ${timeoutSeconds} seconds.`));
    }, timeoutSeconds * 1000);

    child.on("error", error => {
      clearTimeout(timer);
      reject(error);
    });

    child.on("close", code => {
      clearTimeout(timer);
      resolve(code ?? 1);
    });
  });
}

function clampInteger(value, min, max, fallback) {
  const parsed = Number.parseInt(value, 10);
  if (!Number.isFinite(parsed)) {
    return fallback;
  }

  return Math.min(max, Math.max(min, parsed));
}

function stringOrDefault(value, fallback) {
  return typeof value === "string" && value.trim().length > 0 ? value.trim() : fallback;
}

function cryptoRandomId() {
  return `${Date.now().toString(36)}-${Math.random().toString(36).slice(2)}`;
}

const server = http.createServer(async (request, response) => {
  try {
    const url = new URL(request.url || "/", `http://${request.headers.host || "localhost"}`);
    console.log(`${new Date().toISOString()} ${request.method} ${url.pathname}`);
    if (request.method === "GET" && url.pathname === "/health") {
      writeJson(response, 200, { status: "ok" });
      return;
    }

    if (request.method !== "POST" || url.pathname !== "/extract") {
      writeJson(response, 404, { error: "not_found" });
      return;
    }

    if (bridgeToken && request.headers["x-codex-bridge-token"] !== bridgeToken) {
      writeJson(response, 401, { error: "unauthorized" });
      return;
    }

    const body = await readBody(request);
    const result = await runCodex(JSON.parse(body || "{}"));
    writeJson(response, 200, result);
  } catch (error) {
    console.error(error instanceof Error ? error.message : String(error));
    writeJson(response, 500, { error: error instanceof Error ? error.message : String(error) });
  }
});

server.listen(port, "0.0.0.0", () => {
  console.log(`Codex CLI bridge listening on 0.0.0.0:${port}`);
});
