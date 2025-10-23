# 一款结合 AI 和 AR 技术的旅行 APP

# 开发目的

一个应付课题的奇怪项目。整体上是个莫名其妙的app。

# 功能

1. 点击屏幕，使用 AR 技术识别地面，展示小机器人。
2. GPS 定位，结合高德地图 API，实现定位与导航。
3. 用户通过语音与 OpenAI API 交流，AI 识别用户的意图，结合定位与导航信息，给用户说明。

# 使用说明

1. 请在 Prefabs/API 预制体中配置您使用的 AI Key 和 高德地图 key 。
2. 因为使用了腾讯云的语音转文本 API ，请在 Scripts/WhisperSpeechToText.cs 中的 TODO 处，配置鉴权信息。
