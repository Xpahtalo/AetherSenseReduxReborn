* Setup backups for configuration.
* Instead of using `Task.Run(async () => await _())` there should be message queue that synchronous code pushes to, and an async loop is watching to pull from.
    - I'll probably look at MQTTnet for ideas, because that implementation has worked well in ffxiv2mqtt.
* Add Pattern viewer and complex patterns.
* Add helper for target selection.
