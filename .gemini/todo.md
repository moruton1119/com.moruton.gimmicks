# 作業ログ

## URL調査結果

### 1. Upstash Context7 (<https://github.com/upstash/context7>)

- MCPサーバーパッケージ: `@upstash/context7-mcp`
- インストール: `npx -y @upstash/context7-mcp`
- 必須: 特になし（API Key推奨）
- 設定:

```json
{
  "mcpServers": {
    "context7": {
      "command": "npx",
      "args": ["-y", "@upstash/context7-mcp", "--api-key", "YOUR_KEY"]
    }
  }
}
```

### 2. GitHub MCP Server (<https://github.com/github/github-mcp-server>)

- インストール: Docker推奨
- 必須: Docker Desktop, GitHub Personal Access Token (PAT)
- 設定:

```json
{
  "mcpServers": {
    "github": {
      "command": "docker",
      "args": ["run", "-i", "--rm", "-e", "GITHUB_PERSONAL_ACCESS_TOKEN", "ghcr.io/github/github-mcp-server"],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "YOUR_PAT"
      }
    }
  }
}
```

### 3. n8n (<https://github.com/n8n-io/n8n>)

- 状況: 公式READMEにMCPサーバーとしての記述なし。
- n8n自体のインストールは `npx n8n` またはDocker。
- MCP対応については別途確認が必要。

### 4. Perplexity MCP Server (<https://github.com/perplexityai/modelcontextprotocol>)

- MCPサーバーパッケージ: `@perplexity-ai/mcp-server`
- インストール: `npx -y @perplexity-ai/mcp-server`
- 必須: Perplexity API Key
- 設定:

```json
{
  "mcpServers": {
    "perplexity": {
      "command": "npx",
      "args": ["-y", "@perplexity-ai/mcp-server"],
      "env": {
        "PERPLEXITY_API_KEY": "YOUR_KEY"
      }
    }
  }
}
```
