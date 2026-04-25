const http = require("node:http");
const { spawn } = require("node:child_process");
const fs = require("node:fs/promises");
const os = require("node:os");
const path = require("node:path");

const port = Number.parseInt(process.env.PORT || "4517", 10);
const bridgeToken = process.env.CODEX_BRIDGE_TOKEN || "";
const defaultCommand = process.env.CODEX_COMMAND || "codex";
const loginSessions = new Map();

function writeJson(response, statusCode, body) {
  const json = JSON.stringify(body);
  response.writeHead(statusCode, {
    "content-type": "application/json; charset=utf-8",
    "content-length": Buffer.byteLength(json)
  });
  response.end(json);
}

function writeHtml(response, statusCode, body) {
  response.writeHead(statusCode, {
    "content-type": "text/html; charset=utf-8",
    "content-length": Buffer.byteLength(body)
  });
  response.end(body);
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
  const result = await getLoginStatus(command);
  if (!result.loggedIn) {
    throw new Error(`Codex CLI is not logged in inside the codex-cli container. ${result.message}`);
  }
}

async function getLoginStatus(command) {
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
  const message = (stdout || stderr || (exitCode === 0 ? "Logged in" : "Not logged in")).trim();
  return { loggedIn: exitCode === 0, message };
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

function startLogin(command) {
  const sessionId = cryptoRandomId();
  const child = spawn(command, ["login", "--device-auth"], {
    stdio: ["pipe", "pipe", "pipe"],
    env: process.env
  });

  const session = {
    id: sessionId,
    output: "",
    url: null,
    exitCode: null,
    createdAt: new Date().toISOString()
  };

  const appendOutput = chunk => {
    session.output += stripAnsi(chunk);
    session.output = session.output.slice(-20_000);
    session.url ??= extractFirstUrl(session.output);
  };

  child.stdout.setEncoding("utf8");
  child.stderr.setEncoding("utf8");
  child.stdout.on("data", appendOutput);
  child.stderr.on("data", appendOutput);
  child.on("close", code => {
    session.exitCode = code ?? 1;
  });
  child.on("error", error => {
    session.output += `\n${error.message}`;
    session.exitCode = 1;
  });

  loginSessions.set(sessionId, { session, child });
  setTimeout(() => {
    const entry = loginSessions.get(sessionId);
    if (entry && entry.session.exitCode === null) {
      entry.child.kill("SIGTERM");
    }
    loginSessions.delete(sessionId);
  }, 10 * 60 * 1000);

  return session;
}

function extractFirstUrl(text) {
  const match = text.match(/https:\/\/[^\s"'<>]+/i);
  return match ? match[0] : null;
}

function stripAnsi(text) {
  return String(text).replace(/\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])/g, "");
}

function renderLoginPage() {
  return `<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Codex CLI Login</title>
  <style>
    body { font-family: system-ui, -apple-system, Segoe UI, sans-serif; margin: 2rem; max-width: 920px; color: #1f2937; }
    button, a.button { border: 0; border-radius: .5rem; background: #0d6efd; color: white; padding: .7rem 1rem; text-decoration: none; cursor: pointer; display: inline-block; }
    button.secondary { background: #475569; }
    pre { background: #0f172a; color: #dbeafe; padding: 1rem; border-radius: .6rem; white-space: pre-wrap; overflow-wrap: anywhere; }
    .ok { color: #047857; font-weight: 700; }
    .bad { color: #b91c1c; font-weight: 700; }
    .muted { color: #64748b; }
  </style>
</head>
<body>
  <h1>Codex CLI Login</h1>
  <p>This page starts <code>codex login --device-auth</code> inside the Docker <code>codex-cli</code> container. Device authentication avoids localhost callback failures between the host browser and the container.</p>
  <p class="muted">The browser may block popups. If that happens, use the authentication URL and code shown below.</p>
  <p id="status">Checking login status...</p>
  <p>
    <button id="start">Start login and open authentication page</button>
    <button id="refresh" class="secondary">Refresh status</button>
  </p>
  <p id="authLink"></p>
  <div id="deviceWarning" class="bad"></div>
  <pre id="output">No login session started.</pre>
  <script>
    let sessionId = null;
    let authWindow = null;
    let openedUrl = null;
    async function refreshStatus() {
      const response = await fetch('/login/status');
      const status = await response.json();
      document.getElementById('status').innerHTML = status.loggedIn
        ? '<span class="ok">Logged in</span>: ' + escapeHtml(status.message || '')
        : '<span class="bad">Not logged in</span>: ' + escapeHtml(status.message || '');
    }
    async function pollSession() {
      if (!sessionId) return;
      const response = await fetch('/login/session/' + encodeURIComponent(sessionId));
      const session = await response.json();
      renderSession(session);
      if (session.exitCode === null) {
        setTimeout(pollSession, 1500);
      } else {
        if (!session.url && authWindow) {
          authWindow.document.body.innerHTML = '<p>Codex CLI did not return an authentication URL.</p><pre>' + escapeHtml(session.output || 'No output.') + '</pre><p>Please return to the login helper tab.</p>';
        }
        if (!session.url) {
          showDeviceAuthFailure(session.output);
        }
        refreshStatus();
      }
    }
    function renderSession(session) {
      document.getElementById('output').textContent = session.output || 'Waiting for Codex CLI output...';
      if (session.url) {
        document.getElementById('authLink').innerHTML = '<a class="button" target="_blank" rel="noopener" href="' + session.url + '">Open authentication page</a>';
        if (authWindow && openedUrl !== session.url) {
          authWindow.location = session.url;
          openedUrl = session.url;
        }
      }
    }
    function showDeviceAuthFailure(output) {
      const warning = output && output.includes('403 Forbidden')
        ? 'Device authentication is currently rejected with 403 Forbidden. API key login is disabled by project policy.'
        : 'Device authentication did not produce a URL. API key login is disabled by project policy.';
      document.getElementById('deviceWarning').textContent = warning;
    }
    function escapeHtml(value) {
      return String(value).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    }
    document.getElementById('start').addEventListener('click', async () => {
      document.getElementById('output').textContent = 'Starting codex login...';
      authWindow = window.open('', '_blank');
      if (authWindow) {
        authWindow.document.write('<p>Waiting for Codex CLI authentication URL...</p>');
      }
      const response = await fetch('/login/start', { method: 'POST' });
      const session = await response.json();
      sessionId = session.id;
      renderSession(session);
      if (session.url) {
        if (authWindow) {
          authWindow.location = session.url;
        } else {
          window.open(session.url, '_blank', 'noopener');
        }
        openedUrl = session.url;
      }
      pollSession();
    });
    document.getElementById('refresh').addEventListener('click', refreshStatus);
    refreshStatus();
  </script>
</body>
</html>`;
}

const server = http.createServer(async (request, response) => {
  try {
    const url = new URL(request.url || "/", `http://${request.headers.host || "localhost"}`);
    console.log(`${new Date().toISOString()} ${request.method} ${url.pathname}`);
    if (request.method === "GET" && url.pathname === "/health") {
      writeJson(response, 200, { status: "ok" });
      return;
    }

    if (request.method === "GET" && url.pathname === "/login") {
      writeHtml(response, 200, renderLoginPage());
      return;
    }

    if (request.method === "GET" && url.pathname === "/login/status") {
      writeJson(response, 200, await getLoginStatus(defaultCommand));
      return;
    }

    if (request.method === "POST" && url.pathname === "/login/start") {
      writeJson(response, 200, startLogin(defaultCommand));
      return;
    }

    const loginSessionMatch = url.pathname.match(/^\/login\/session\/([^/]+)$/);
    if (request.method === "GET" && loginSessionMatch) {
      const entry = loginSessions.get(loginSessionMatch[1]);
      if (!entry) {
        writeJson(response, 404, { error: "login_session_not_found" });
        return;
      }

      writeJson(response, 200, entry.session);
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
