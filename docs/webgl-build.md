# WebGL 构建与部署指南（星岛计划 / StarIsle）

本文档说明如何把《星岛计划》打包为 WebGL，并嵌入作品集站点 `web/index.html`。

## 一、前提

- 已用 **Unity 2022.3.62** 打开工程，并能正常进入 Play（控制台无错误）。
- 场景中已放置一个挂载 `GameManager` 的空 GameObject（它会自举挂接 EcoSim / ResourceSystem / BioSystem / SaveSystem / Hud / InputRouter）。
- 已安装 **WebGL Build Support** 模块（Unity Hub → 安装 → 2022.3.62 → 添加模块 → WebGL Build Support）。

## 二、Build Settings

1. 菜单 `File > Build Settings`。
2. 左侧平台选 **WebGL** → 点 `Switch Platform`（切换需数十秒）。
3. 点 `Player Settings`，逐项确认：
   - **Resolution and Presentation**
     - `Run in background` ✅ 勾选（标签页切走后继续模拟）。
     - `WebGL Template`：选 `Default` 即可（自定义外壳用本仓库 `web/index.html`，不依赖模板）。
   - **Publishing Settings**
     - `Compression Format`：**Brotli**（体积最小；需服务器支持 `.br` 或开 `Decompression Fallback`）。
     - `Decompression Fallback` ✅ 勾选（用 JS 解压，避免服务器 MIME 配置问题，最稳）。
     - `Data Caching` ✅ 勾选（二次访问走缓存）。
   - **Other Settings**
     - `Color Space`：选 **Linear**（URP 默认；若 WebGL 上偏色可改 Gamma 试）。
     - `Scripting Define Symbols`：保持现状（2022.3 下**不要**加 `STARISLE_SENTIS`，Sentis 端侧推理仅 Unity 6 支持）。
     - `Api Compatibility Level`：`.NET Standard 2.1`。
4. 回到 Build Settings，点 `Build`。
5. 输出目录选择 **`web/build`**（即仓库内 `web/build/`）。Unity 会生成：
   ```
   web/build/
     index.html
     Build/
       <项目名>.loader.js
       <项目名>.framework.js
       <项目名>.data
       <项目名>.wasm
     TemplateData/
       ...
   ```

> ⚠️ 若 `web/build` 已存在旧产物，先清空再 Build，避免缓存混淆。

## 三、本地预览

最简单：用任意静态服务器打开 `web/` 目录（不要直接双击 `index.html`，`file://` 会因 CORS 失败）。

```bash
# 任选其一，在仓库根目录执行
cd web && python3 -m http.server 8080
# 或
npx serve web
```

浏览器访问 `http://localhost:8080/`，《星岛计划》应自动加载并进入试玩区（页面 iframe 指向 `build/index.html`）。

## 四、部署（上线作品集）

构建产物是纯静态文件，可托管到任意静态空间：

- **GitHub Pages**：把 `web/` 内容推到仓库，Pages 源设为该目录。
- **Vercel / Netlify / Cloudflare Pages**：根目录设为 `web`，构建命令留空，输出 `web`。
- **自有服务器**：整目录上传，`nginx` 需为 `.br`/`.wasm` 配正确 MIME（`application/wasm`、`application/octet-stream`），或直接开 `Decompression Fallback` 规避。

## 五、WebGL 兼容性已核验

仓库脚本已做过 WebGL 安全审计，确认**未使用**以下 WebGL 不支持的 API：
`System.IO` 文件读写、`Thread`/任务并行、`Socket`/`Process`/`DllImport` 等。
`SaveSystem` 使用 `PlayerPrefs`（WebGL 安全，底层为 IndexedDB），可直接发布。

## 六、性能与体积提示

- 首次加载会下载 `.data`（资源包），建议开 Brotli + Data Caching。
- 中低配目标 60fps：当前为 Low Poly + URP，开销可控；若后续加模型/贴图，注意控制 `.data` 体积（< 50MB 体验更佳）。
- WebGL 下避免使用 `OnGUI` 重绘（本项目已迁 UIToolkit，符合）。

## 七、已知限制（本切片）

- 生物 AI 在 2022.3 下走**嵌入式 C# 神经网络推理**（纯前向，零依赖），行为真实但非 GPU 加速。
- 切回 Unity 6 + 安装 Sentis 后，加 `STARISLE_SENTIS` 编译符号即可切换为 GPU 端侧推理，行为一致。
- 存档为 `PlayerPrefs`（WebGL 即浏览器本地），换设备不互通，符合作品集展示定位。
