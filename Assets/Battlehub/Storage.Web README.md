## Web Storage Sample

You can use the runtime asset database in conjunction with an HTTP web server, in WebGL and Standalone builds.

To start the web sample, follow these steps:

1. Install the [Newtonsoft Json package]((https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html)). In Package Manager, click on "+ Add package by name" and enter "com.unity.nuget.newtonsoft-json"
2. Unpack the `Asset/Battlehub/Storage.Web` Unity package.
3. Open the `Asset/Battlehub.Extensions/Storage.Web/SampleScene` Unity scene.
4. Extract `Asset/Battlehub.Extensions/Storage.Web/SampleHttpServer.zip` to a folder (e.g., `C:\SampleHttpWebServer`).
5. Install Node.js from [Node.js](https://nodejs.org/en/learn/getting-started/how-to-install-nodejs).
6. Open a terminal and navigate to `C:\SampleHttpWebServer`.
7. Run the command `npm install`.
8. Run the command `node app.js`.
9. Enter play mode in Unity.
10. The projects will be created in the `Project Root Folder Path` (see SampleScene/RuntimeEditor/Runtime Editor (Script)/Extra Settings/Projects Root Folder Path).
