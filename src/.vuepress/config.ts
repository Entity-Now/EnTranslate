import { defineUserConfig } from "vuepress";
import theme from "./theme.js";

export default defineUserConfig({
  base: "/",

  lang: "zh-CN",
  title: "EnTranslate",
  description: "EnTranslate一款纯粹的Visual Studio 2022 VSIX划词翻译插件",

  theme,

  // Enable it with pwa
  // shouldPrefetch: false,
});
