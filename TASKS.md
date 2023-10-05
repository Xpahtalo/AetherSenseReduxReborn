* Add saving configuration. Has to be simple to migrate versions. Potentially look at Mare. Prefer System.Text.Json over Newtonsoft.Json.
* Everything outside of ButtplugWrapper.cs should only care about device attributes and not be aware of devices specifically.
    - Current thoughts are to hash a string of the device name, and all attribute information.
    - Calling attributes commands would just be done by hash then, and ButtplugWrapper works out exactly which device and attribute to send it to
* All signal groups should start disabled, and when new devices are detected, check the list to see if the attribute they were last assigned to have been added.
    - If so, re-enable the signal group.
* Having the signal config saved inside the Signal is fucked. Need to adjust this to be less annoying to work with. No ideas yet.
* Instead of using `Task.Run(async () => await _())` there should be message queue that synchronous code pushes to, and an async loop is watching to pull from.
    - I'll probably look at MQTTnet for ideas, because that implementation has worked well in ffxiv2mqtt.
