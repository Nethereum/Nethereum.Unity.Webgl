# Nethereum.Unity.Webgl
Nethereum Unity Webgl sample starter (WIP)

+ All working but needs refactoring etc (see future thoughts)
+ Entry point is the Metamask Controller: https://github.com/Nethereum/Nethereum.Unity.Webgl/blob/main/Assets/MetamaskController.cs
+ Only supported in 2021 and above due to emscripten


## Future thoughts, next steps:
+ Refactor out to the core Nethereum.Unity library where possible
+ Create an interceptor, mainly a way to switch between MM, normal Nethereum or other provider to do requests, this way your code does not need change depending on your environment (console, desktop, mobile, webgl)
+ Figure out a better way to handle call backs, (maybe wait for a concurrent queue of responses held statically) ideally within the interop service, so interaction is the same as other calls (see above)

![MMUnity2021 gif](https://user-images.githubusercontent.com/562371/148795418-d09438d0-5857-4dfc-92af-3a3b025f8c22.gif)
