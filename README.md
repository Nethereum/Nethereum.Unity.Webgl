# Nethereum.Unity.Webgl
Nethereum Unity Webgl sample starter (WIP)

**Notes and thoughts: **
+ This is working, but the next steps will be to:
+ Refactor out to the core Nethereum.Unity library where possible
+ Create an interceptor, mainly a way to switch between MM, normal Nethereum or other provider to do requests, this way your code does not need change depending on your environment.
+ Figure out a better way to handle call backs, (maybe wait for a concurrent queue of responses held statically) ideally within the interop service, so interaction is the same as other calls (see above)

![MMUnity2021 gif](https://user-images.githubusercontent.com/562371/148795418-d09438d0-5857-4dfc-92af-3a3b025f8c22.gif)
