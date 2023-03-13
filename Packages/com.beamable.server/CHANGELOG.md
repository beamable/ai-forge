# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.11.1]
### Fixed
- `InventoryView` is serializable.

## [1.11.0]
### Added
- Microservices with `IFederatedLogin<T>` will generate client callable methods.
- Microservices with `IFederatedInventory<T>` will generate client callable methods.

### Fixed
- Empty dictionary of supported subtypes no longer break request serialization.
- Check for missing dependencies before microservices deploy
- `GetManifestWithID` method works for manifest ids other than `"global"`. 
- Microservice path names are case insensitive 

## [1.10.3]
### Fixed
- Memory leak no longer occurs while handling requests. `IDependencyProvider` disposal frees memory.

## [1.10.2]
### Fixed
- Microservices can now deploy in realms with no deployed Content manifest.

## [1.10.1]
no changes

## [1.10.0]
### Added
- `[InitializeService]` exposes `IDependencyProvider` as `Provider`, and `[ConfigureServices]` exposes `IDependencyBuilder` as `Builder`

### Changed
- Internal dependency-injection system uses `IDependencyBuilder` and `IDependencyProvider`. 
- Websocket connection recovery log level changed from Error to Debug.
- Microservices can override their health check port with the `HEALTH_PORT` env variable.

### Fixed
- Custom `[InitializeService]` and `[ConfigureServices]` callbacks no longer run for each connection.
- Singletons registered during `[ConfigureServices]` won't be re-instantiated on each request.

## [1.9.1]
### Changed
- Downloading content allocates less memory due to avoid async/await `Task` allocation.

## [1.9.0]
### Added
- `Context.ThrowIfCancelled()` method to force end a client-callable request if it has timed out.

### Fixed
- Internal container health checks no longer cause fatal exception.
- `IContentApi` is accessible via the Microservice dependency injection scope.

## [1.8.0]
### Added
- `EnableEagerContentLoading` configuration setting on `MicroserviceAttribute` is enabled by default. 

### Changed
- Content is downloaded and cached on the Microservice before it is declared healthy and available to accept traffic. 
- Published Microservices open 10 websocket connections instead of 30.

### Fixed
- Content downloads no longer cause HTTP timeouts or CPU spikes.
- Domain Reload times are reduced by roughly 30% when working with Microservices
- Rare authorization locking bug that could cause extend authorization times.

## [1.7.0]
### Changed
- Exposed methods for access to public player stats:
  - `GetPublicPlayerStat`
  - `GetPublicPlayerStats`
  - `GetAllPublicPlayerStats`
- Microservice request context body and header properties are lazily deserialized.
- Deployed Microservices run multiple local instances to improve reliability and performance. 
- Microservices no longer represent inbound messages with an intermediate `string`. Instead, messages byte arrays are parsed directly to `JsonDocument` 

### Added
- Microservice message log size limit. 
- Inbound requests are rate limited to avoid out of memory failures. 

### Removed
- Microservice log messages no longer include message hash, `"__i"` field.
- Microservices no longer emit log body and headers on every log statement.
- Microservices no longer emit log messages for receiving and responding to `[ClientCallable]` methods.

## [1.6.2]
### Changed
- The `Publish Window` is now centered on show relative to the editor

### Fixed
- There can only be one instance of the `Publish Window` in the `Microservice Manager`


## [1.6.1]
- no changes

## [1.6.0]
### Added
- Displaying log pagination if message contains more that 5000 chars
- Quick action buttons for opening C# code and local documentation for service cards
- Copy button to copy full log message

### Changed
- New `Microservice Manager` UI styling 
- Play-all button request microservice selection
- New service icons in `Publish Window`

### Fixed
- Various `DockerNotInstalledException` events when MicroserviceManager window isn't open, but Docker ins't running.
- `Microservice Manager` no longer freezes when log has more than 65535 vertices

### Removed
- Checkboxes and local status icon on microservice cards

## [1.5.2]
### Fixed
- `curl error 52` while publishing Microservices and performing health-checks.
- Task Cancellation exceptions while publishing Microservice.
- Various `DockerNotInstalledException` events when MicroserviceManager window isn't open, but Docker ins't running.

## [1.5.1]
### Added
- `EnablePrePublishHealthCheck` option in _Project Settings/Beamable/Microservices_ can be used to disable Microservice health checks when publishing. Disabling this is dangerous and may lead to unhealthy servers being deployed to production.
- `PrePublishHealthCheckTimeout` option in _Project Settings/Beamable/Microservices_ can optionally override the amount of seconds before a health check is considered to timeout. The default value is 10 seconds.

## [1.5.1]
### Added
- `EnablePrePublishHealthCheck` option in _Project Settings/Beamable/Microservices_ can be used to disable Microservice health checks when publishing. Disabling this is dangerous and may lead to unhealthy servers being deployed to production.
- `PrePublishHealthCheckTimeout` option in _Project Settings/Beamable/Microservices_ can optionally override the amount of seconds before a health check is considered to timeout. The default value is 10 seconds.

## [1.5.0]
### Fixed
- `SequenceContainsNoElements` error when building Microservices.

## [1.4.0]
### Added
- Added `long` PlayerId version of `InviteToParty`, `PromoteToLeader` and `KickPlayer` methods of the `IPartyApi` interface.
- Utility APIs for setting expiration on `MailUpdate` and `MailSend` requests

### Fixed
- ActionBarVisualElement buttons behaviour is fixed when Docker is not running.
- Fixed issue with MS rebuild/stop on entering to Playmode.
- Fixed Microservices stop at Unity Exit.
- DependencyResolver allows possibility to remove asmdef reference by user.

## [1.3.6]
### Added
- Allow disabling `System.Runtime.CompilerServices.Unsafe.dll` inclusion by using `BEAMABLE_DISABLE_SYSTEM_COMPILERSERVICES` define symbol

## [1.3.5]
no changes

## [1.3.4]
no changes

## [1.3.3]
### Changed
- Changed service name validation in `Microservice Manager` to keep names unique

## [1.3.2]
### Added
- Added `Services.Payments` which allows receipt verification.
- Added `DeleteProtectedPlayerStats` and `DeleteStats` methods to `IMicroserviceStatsApi`.

### Fixed
- Manually adding a `StorageObject` Assembly Definition as a dependency of a `Microservice`'s Assembly Definition now correctly sets up all the necessary Mongo DLLs for the `StorageObject` to be usable inside the Microservice. You can disable this behaviour by setting `MicroserviceConfiguration.EnsureMongoAssemblyDependencies = false`. The recommended way to do set service dependencies is still to use the Dependency button of the Microservice Manager Window.

## [1.3.1]
### Added
- `BeamableRequestError` to `RequestException` base type that can be used to catch exception from Microservice requests to Beamable.
- A leaderboard can now be frozen using `Services.Leaderboards.FreezeLeaderboard` method to prevent additional scores to be submitted.
- Microservice can include a `CsProjFragment.xml` file as a `.csproj` `<ItemGroup>` property block of nuget references that the microservice will use to resolve.
- Added `GetAccountId` method to `IMicroserviceAuthApi` that returns the requesting user's AccountId as opposed to their GamerTag.

### Fixed
- Publish doesn't fail if there is an unused StorageObject entry in the MicroserviceConfiguration
- Microservices reload route table after hot module reload code change. 
- Microservices can accept `InventoryUpdateBuilder` and other types that include subclasses of `SerializableStringTo<T>`
- Microservices stop stale containers before rebuilding.
- Microservices recognize build failure vs success correctly during local development.
- Deployed Microservices will restart if they fail to re-authenticate with Beamable
- Reference `dll` file no longer copies parent directory

### Changed
- Microservices use the docker `-v` flag to specify bind mounts instead of `--mount`.
- Microservices may not be published as ARM images. Microservices will be forced to "linux/amd64" architecture.

## [1.3.0]
### Added
- User can specify Microservices build and deploy CPU architecture. 
- `RemovePlayerEntry` for leaderboards API which allows to remove given player from the leaderboard
- Microservices have their initialization validated before publishing. 
- Microservice archive/unarchive feature.
- Basic Chat SDK functions to Microservice
- The base docker image used for Microservices and Microstorages will be automatically pulled at startup.
- Client Generator logs go to the Microservice Window
- Send Microservice CPU architecture to Beamable Cloud
- Headers are available on the service `Context` for application version, unity version, game version, and Beamable sdk version

### Changed
- local microservice logs will appear for dotnet watch command
- Microservices use a Nuget cache for faster development builds
- Microservices cache their `dotnet restore` output in the Docker cache for faster development builds
- Microservices share a realm secret request for faster development builds
- Local microservices no longer output emoji characters from their `dotnet watch` command
- Microservices only receive events for content updates
- Disabled Microservices no longer get built and published.

### Fixed
- Microservice related actions can run while Unity is a background process.
- Microservice clients created by using the default constructor will now keep working after the default `BeamContext` has been reset.
- Local Microservices no longer say "could not find servicename:latest"
- Publish flow locks Asset Database so that no re-imports may happen.
- Publish screen loading bar should always be full when publish is complete.
- Fixed problems with unexited OS processes and high memory consumption for Docker during switch between EditMode and PlayMode.
- The "Play Selected" button in the Microservice window doesn't get stuck in a service is already running.
- Microservice selection is saved between domain reloads.
- Microservice paths can now contain spaces.
- Compile errors are reported as error logs

### Removed
- Unused legacy code around "Auto Run Local Microservices" menu item

## [1.2.10]
### Added
- `DisableAllBeamableEvents` option for the `MicroserviceAttribute`. When enabled, prevents the Microservice from receiving any Beamable events, including content cache invalidations.

## [1.2.9]
### Fixed
- Potential microservice issue that caused C#MSs to hang during initialization.

## [1.2.8]
no changes

## [1.2.7]
no changes

## [1.2.6]
### Added
- `RemovePlayerEntry` for leaderboards API which allows to remove given player from the leaderboard

### Fixed
- Microservices may be built from either ARM or x86 based computers and uploaded to Beamable.

## [1.2.5]
### Fixed
- Failed promises no longer log exception info after an exception handler is registered on the same execution cycle.
- "Connection is closed" log exception no longer prints incorrectly.
- Requests no longer attempt to send while authorization process is happening.


## [1.2.4]
### Added
- Microservices now support private declarations of `Callable` methods.
- Added log notifying users that Microservices don't currently support overloaded `Callable`. 

### Fixed
- Microservices now properly log exceptions that happen during its initialization
- Microservice process commands now use the `BeamableDispatcher` instead of the `EditorApplication.delayCall`. This allows you to background Unity during long running microservice actions.
- Issue in Microservices re-auth flow that caused high CPU utilization unnecessarily

## [1.2.3]
### Added
- `UnityEngine.Debug.LogFormat` now supported when used inside C#MS methods 

### Changed
- Socket re-authorization flow uses a spinlock mechanism instead of a mutex

### Fixed
- Socket re-connection waits for the socket to reconnect before yielding the task scheduler

## [1.2.2]
### Fixed
- Fixed microservices build issue on Mac with ARM CPU architecture

## [1.2.1]
### Fixed
- Microservices now correctly caches connection strings when `GetDatabase` is called on the `IStorageObjectConnectionProvider` service.  
- Possible duplicate authorization requests.
- Messages sent during a re-connection event will be re-attempted 10 times before failing.

## [1.2.0]
### Added
- Support for GUID based assembly references.
- `CallableAttribute` for exposing C#MS methods that are meant to be publicly accessible (without authentication required).
- `ListLeaderboards` method to `IMicroserviceLeaderboardsApi` will return lists of leaderboard ids.
- `GetPlayerLeaderboards` method to `IMicroserviceLeaderboardsApi` will return leaderboards for a given player.
- `lbId` field to the `LeaderboardView` response class.
- `DisableDockerBuildkit` property to the MicroserviceConfiguration. By default, Docker buildkit will now be enabled.

### Fixed
- Client code can handle receiving a `ContentObject` response from a `ClientCallable`.
- Removed Microstorage related null reference errors on Unity startup.
- `IMicroserviceNotificationsApi` can now send strings with spaces in them for messages.
- `IMicroserviceLeaderboardsApi` will now respect `HasValue` flag of `Optional<T>` derived types in all cases.
- Fixed issue with Publish flow that caused an invalid Manifest data to exist when publishing any services along a service whose source code was no longer in the project
- Fixed issue that made it possible to start a remote service without its dependencies up and running (only happened in cases where the service was only remote --- ie: the source code for it was not present in the project)   

### Changed
- Building microservices will always pull the latest version of dependent alpine linux Docker base images.
- `ClientCallableAttribute` is now only accessible to authenticated users. For a fully public endpoint, use `CallableAttribute` instead.   
- Microservices will be built specifically for linux/amd64 architecture. For developers with ARM based CPU architectures, enable to the `DockerBuildkit` setting in the Microservice Configuration to publish microservices. 
- Building a microservice will always stop the microservice and its source generator if they are running. After the build, the source generator will be reset.

### Removed
- `EnableDockerBuildkit` property from the MicroserviceConfiguration. By default, Docker buildkit will now be enabled. Disable it again with the new `DisableDockerBuildkit` field.

## [1.1.4]
### Fixed
- Thrown `MicroserviceExceptions` from `[ClientCallable]` methods will result in an appropriate error response.

## [1.1.3]
no changes

## [1.1.2]
### Fixed
- boolean types are now supported in swagger documentation

## [1.1.1]
### Changed
- Realm switch now triggers `Microservice Manager` to stop all active services. Guarantees the correct service version association with realm.

## [1.1.0]
### Added
- `BeamServicesCodeWatcher` detects any change that makes it necessary to rebuild C#MS images as well as cleaning up Auto-Generated files whenever a C#MS AsmDef is deleted. This makes the easiest way to delete a C#MS's code from your project simply to delete it's folder. 
- Beam hint that warn users entering play-mode that there are stale services that must be rebuilt. Avoids wasting time when making quick changes to microservices and forgetting to regenerate the local image during development.
- `EnableAutoPrune` configuration setting that will remove old unused docker image layers. This should limit the disk space requirements of Beamable Microservices on developer machines.
- `EnableHotModuleReload` configuration setting that will enable dotnet 6 hot module reloading for all Microservices.
- Added `IMicroserviceNotificationApi` to list of services accessible from `ClientCallable` and `AdminOnlyCallable` methods of Microservices. These can be used for server-to-client communication.
- `RiderDebugTools` configuration setting to preload Rider debugging tools onto Microservice development images

### Changed
- When exiting Unity, all related Microservices and Microstorage containers are closed
- Microservice client code is generated in a dockerized dotnet runtime instead of Unity
- Added docstrings to `StatsService.SearchStats` to clarify correct usage of the `Criteria` parameter.
- `AssumeUser` takes an optional boolean parameter to disable the Admin access token check
- Service name must be a valid C# class without culture-specific characters
- Updated Microservice Publish window UI/UX

### Fixed
- Fixed issue that caused the `ReflectionCache` to run an extra unnecessary time when a `.cs` or `.asmdef` file were changed.
- Fixed issue on Re-Import All with `BeamableAssistantWindow` opened that required reopening the window for it to work.
- Fixed issue that caused `StatsService.SearchStats` to fail whenever a match occurred.
- Cannot create invalid service name before validation occurs
- Progress bar in publish window now correctly displays the progress of services publication
- Client code can handle receiving a `null` response from a `ClientCallable`

## [1.0.8]
no changes

## [1.0.7]
This is a broken package. It includes changes from the 1.1.0 release. Please do not use this version.

## [1.0.6]
no changes

## [1.0.5]
no changes

## [1.0.4]
no changes

## [1.0.3]
no changes

## [1.0.2]
### Fixed
- Windows Microservices first time build issue regarding empty build directories

## [1.0.1]
### Fixed
- `StorageDocument` types are assigned ID values during replace operations
- UI glitches in the Microservice Manager window

## [1.0.0]
### Added
- `StorageDocument` base class for storage data classes that automatically handle document ID assignment.
- Automatic Mongo serialization for basic Unity structs like `Vector2`, `Color`, and `Quaternion`
- Automatically generate a client-server shared asmdef, and new Microservices automatically reference it

### Changed
- Upgraded Microservices to dotnet 6.0 instead of 5.0
- Microstorage is out of Preview. Storage Objects can now be published and used in a remote environment.
- Microstroage `GetCollection` method must now take subclass of `StorageDocument`
- Return values from `ClientCallable` methods are serialized using Unity style serialization
- Microservice Publish window has improved performance and User Experience

### Fixed
- Swagger docs handle generic types instead of failing to load
- C#MS Log View stay attached to the bottom of the scroller

## [0.18.2]
### Fixed
- Typless `Promise` in `ClientCallable` methods
- Autogenerated client code errors for generic class and arrays. 

## [0.18.1]
### Added
- Support for returning `Promise` from `ClientCallable` method

### Fixed
- Autogenerated client code null reference exception and list errors. 

## [0.18.0]
### Added
- `ApiContent` classes and execution methods allow you to invoke microservices with data
- `InitializeServicesAttribute` can now be used over static methods to declare initialization hooks in microservices. Supported signatures are async/regular `
  Task(IServiceInitializer)`, async/regular `Promise<Unit>(IServiceInitializer)` and synchronous `void(IServiceInitializer)`.
  `void` methods must be fully synchronous --- it is not possible to guarantee that any promises started within a `void` initialization
  method will have completed by the time the microservice is receiving traffic.
- Exposed `CreateLeaderboard` methods in `IMicroserviceLeaderboardsApi` to enable the dynamic creation of leaderboards in microservices (can take a `LeaderboardRef` as a template or explicit parameters).
- Folding/Unfolding services cards in `Microservice Manager`
- Added clearer unsupported message for microservice's implementation of `IAuthService.GetUser(TokenResponse)`
- Added support for `Dictionary` serialization in `ClientCallable` methods using `SmallerJSON`

### Changed
- Renamed `build and run` to `play` buttons in `Microservice Manager` to be more intuitive
- Can have multiple `ConfigureServicesAttribute` and `InitializeServicesAttribute` explicitly ordered via `ExecutionOrder` property of the attributes.

## [0.17.4]
### Fixed
- Publish loading bars

## [0.17.4]
### Fixed
- Publish loading bars

## [0.17.3]
### Added
- Add `IMicroserviceStatsApi.GetAllProtectedPlayerStats` method without filtering stats

### Fixed
- Filters now work for `IMicroserviceStatsApi.GetProtectedPlayerStats`

## [0.17.2]
### Fixed
- Store Microservice window height between reloads
- Serialization support for `Vetor2Int` and `Vector3Int` as input parameters to `ClientCallable` methods
- Serialization support for raw JSON strings as input parameters to `ClientCallable` methods

## [0.17.1]
- no changes

## [0.17.0]
### Added
- Local Mongo Storage Preview
- Ability to use Promises as a return type for `ClientCallable` methods
- RemoteOnly Microservices visible in Miscroservice window
- Deployed Microservice instances will be automatically re-run if they become unhealthy

### Fixed
- Microservice clients can now deserialize Json lists
- Microservice log view stays focused on bottom of log feed

### Changed
- Generated services no longer include the class name in the namespace

## [0.16.0]
### Added
- New Microservices Manager window
- Local microservice health checks are accessible on container port 6565
- Snyk testing for microservices
- Ability to categorize `[ClientCallable]` methods in documentation with `[SwaggerCategory]`

### Changed
- Beamable Platform errors all extend from `RequesterException` in Unity Client and microservice code

### Fixed
- Visual Studio Code debug configuration source maps are now correct
- `AssumeUser()` no longer throws null reference

## [0.15.0]
### Added
- Commerce Service AccelerateListingCooldown support to reduce the cooldown on a listing


## [0.14.3]
- JSON Serialization error in Stats API
- `AssumeUser()` no longer throws null reference


## [0.14.2]
### Fixed
- Namespace declarations starting with Editor, and removed un-used class PopupBtn
 

## [0.14.1]
- no changes


## [0.14.0]
### Added
- Dependency Injection system
- ISerializationCallbackReceiver for request and response objects used in `[ClientCallable]` methods
- Custom routing prefix setting in `MicroserviceConfiguration`

## [0.13.0]
### Added
- Added a difference check at Microservice Deployment time, so that microservices that have not changed are not reuploaded.
- Added a RealmConfig service to available Services. This can be used to read protected realm settings.

## [0.11.0]
### Changed
- Changed response serialization from Microservice. Use UseLegacySerialization to maintain old behaviour.

### Added
- Added a cache for content fetches. 
- Added Doc Url to package.json.
- Added Open API 3.0 auto-generative documentation.
- Added AdminOnlyCallable version of ClientCallable that requires caller to have full scopes.
- Added requiredScopes field to ClientCallable that requires caller to have required scopes.

### Fixed
- Fixes leaderboard and stat fetching cache errors.
- Fixes null reference bug when passing null to [ClientCallable] arrays.
