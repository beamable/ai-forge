using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Assistant;
using Beamable.Common.Reflection;
using Beamable.Editor.Environment;
using Beamable.Editor.Microservice.UI.Components;
using Beamable.Editor.UI.Model;
using Beamable.Reflection;
using Beamable.Server.Editor.DockerCommands;
using Beamable.Server.Editor.ManagerClient;
using Beamable.Server.Editor.UI;
using Beamable.Server.Editor.UI.Components;
using Beamable.Server.Editor.Uploader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.Features.Docker;
using static Beamable.Common.Constants.Features.Services;
using static Beamable.Common.Constants.MenuItems.Assets.Orders;
using LogMessage = Beamable.Editor.UI.Model.LogMessage;

namespace Beamable.Server.Editor
{
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(fileName = "MicroserviceReflectionCache", menuName = "Beamable/Reflection/Microservices Cache",
	                 order = MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_2)]
#endif
	public class MicroserviceReflectionCache : ReflectionSystemObject
	{
		[NonSerialized] public Registry Cache;

		public override IReflectionSystem System => Cache;

		public override IReflectionTypeProvider TypeProvider => Cache;

		public override Type SystemType => typeof(Registry);

		private MicroserviceReflectionCache()
		{
			Cache = new Registry();
		}

		public class Registry : IReflectionSystem
		{
			private static readonly BaseTypeOfInterest MICROSERVICE_BASE_TYPE;
			private static readonly BaseTypeOfInterest MONGO_STORAGE_OBJECT_BASE_TYPE;
			private static readonly List<BaseTypeOfInterest> BASE_TYPES_OF_INTEREST;

			private static readonly AttributeOfInterest MICROSERVICE_ATTRIBUTE;
			private static readonly AttributeOfInterest STORAGE_OBJECT_ATTRIBUTE;
			private static readonly List<AttributeOfInterest> ATTRIBUTES_OF_INTEREST;

			static Registry()
			{
				MICROSERVICE_BASE_TYPE = new BaseTypeOfInterest(typeof(Microservice));
				MICROSERVICE_ATTRIBUTE =
					new AttributeOfInterest(typeof(MicroserviceAttribute), new Type[] { },
											new[] { typeof(Microservice) });

				MONGO_STORAGE_OBJECT_BASE_TYPE = new BaseTypeOfInterest(typeof(MongoStorageObject));
				STORAGE_OBJECT_ATTRIBUTE =
					new AttributeOfInterest(typeof(StorageObjectAttribute), new Type[] { },
											new[] { typeof(StorageObject) });

				BASE_TYPES_OF_INTEREST =
					new List<BaseTypeOfInterest>() { MICROSERVICE_BASE_TYPE, MONGO_STORAGE_OBJECT_BASE_TYPE };
				ATTRIBUTES_OF_INTEREST =
					new List<AttributeOfInterest>() { MICROSERVICE_ATTRIBUTE, STORAGE_OBJECT_ATTRIBUTE };
			}

			public List<BaseTypeOfInterest> BaseTypesOfInterest => BASE_TYPES_OF_INTEREST;
			public List<AttributeOfInterest> AttributesOfInterest => ATTRIBUTES_OF_INTEREST;

			private Dictionary<string, MicroserviceBuilder> _serviceToBuilder =
				new Dictionary<string, MicroserviceBuilder>();

			private Dictionary<string, MongoStorageBuilder> _storageToBuilder =
				new Dictionary<string, MongoStorageBuilder>();

			public readonly List<StorageObjectDescriptor> StorageDescriptors = new List<StorageObjectDescriptor>();
			public readonly List<MicroserviceDescriptor> Descriptors = new List<MicroserviceDescriptor>();
			public readonly List<IDescriptor> AllDescriptors = new List<IDescriptor>();

			private IBeamHintGlobalStorage _hintStorage;

			public void SetStorage(IBeamHintGlobalStorage hintGlobalStorage) => _hintStorage = hintGlobalStorage;

			public void ClearCachedReflectionData()
			{
				_serviceToBuilder.Clear();
				_storageToBuilder.Clear();

				Descriptors.Clear();
				StorageDescriptors.Clear();
				AllDescriptors.Clear();
			}

			public void OnSetupForCacheGeneration()
			{
				// Since we don't require any setup prior to generating the cache, we can skip it.
			}

			public void OnReflectionCacheBuilt(PerBaseTypeCache perBaseTypeCache,
											   PerAttributeCache perAttributeCache)
			{
				// TODO: Display BeamHint of validation type for microservices declared in ignored assemblies.
			}

			public void OnBaseTypeOfInterestFound(BaseTypeOfInterest baseType, IReadOnlyList<MemberInfo> cachedSubTypes)
			{
				if (baseType.Equals(MICROSERVICE_BASE_TYPE))
					ParseMicroserviceSubTypes(cachedSubTypes);

				if (baseType.Equals(MONGO_STORAGE_OBJECT_BASE_TYPE))
					ParseStorageObjectSubTypes(cachedSubTypes);

				void ParseMicroserviceSubTypes(IReadOnlyList<MemberInfo> cachedMicroserviceSubtypes)
				{
					var validationResults = cachedMicroserviceSubtypes
						.GetAndValidateAttributeExistence(MICROSERVICE_ATTRIBUTE,
														  info =>
														  {
															  var message =
																  $"Microservice sub-class [{info.Name}] does not have the [{nameof(MicroserviceAttribute)}].";
															  return new AttributeValidationResult(
																  null, info,
																  ReflectionCache.ValidationResultType.Error, message);
														  });

					// Get all Microservice Attribute usage errors found
					validationResults.SplitValidationResults(out _, out _, out var microserviceAttrErrors);

					// Register a hint with all its validation errors as the context object
					if (microserviceAttrErrors.Count > 0)
					{
						var hint = new BeamHintHeader(BeamHintType.Validation,
													  BeamHintDomains.BEAM_CSHARP_MICROSERVICES_CODE_MISUSE,
													  BeamHintIds.ID_MICROSERVICE_ATTRIBUTE_MISSING);
						_hintStorage.AddOrReplaceHint(hint, microserviceAttrErrors);
					}
				}

				void ParseStorageObjectSubTypes(IReadOnlyList<MemberInfo> cachedStorageObjectSubTypes)
				{
					var validationResults = cachedStorageObjectSubTypes
						.GetAndValidateAttributeExistence(STORAGE_OBJECT_ATTRIBUTE,
														  info =>
														  {
															  var message =
																  $"{nameof(StorageObject)} sub-class [{info.Name}] does not have the [{nameof(StorageObjectAttribute)}].";
															  return new AttributeValidationResult(
																  null, info,
																  ReflectionCache.ValidationResultType.Error, message);
														  });

					// Get all Microservice Attribute usage errors found
					validationResults.SplitValidationResults(out _, out _, out var storageObjAttrErrors);

					// Register a hint with all its validation errors as the context object
					if (storageObjAttrErrors.Count > 0)
					{
						var hint = new BeamHintHeader(BeamHintType.Validation,
													  BeamHintDomains.BEAM_CSHARP_MICROSERVICES_CODE_MISUSE,
													  BeamHintIds.ID_STORAGE_OBJECT_ATTRIBUTE_MISSING);
						_hintStorage.AddOrReplaceHint(hint, storageObjAttrErrors);
					}
				}
			}

			public void OnAttributeOfInterestFound(AttributeOfInterest attributeType,
												   IReadOnlyList<MemberAttribute> cachedMemberAttributes)
			{
				if (attributeType.Equals(MICROSERVICE_ATTRIBUTE))
					ParseMicroserviceAttributes(cachedMemberAttributes);

				if (attributeType.Equals(STORAGE_OBJECT_ATTRIBUTE))
					ParseStorageObjectAttributes(cachedMemberAttributes);

				void ParseMicroserviceAttributes(IReadOnlyList<MemberAttribute> cachedMicroserviceAttributes)
				{
					// Searches for all unique name collisions.
					var uniqueNameValidationResults = cachedMicroserviceAttributes
						.GetAndValidateUniqueNamingAttributes<MicroserviceAttribute>();

					// Registers a hint with all name collisions found.
					if (uniqueNameValidationResults.PerNameCollisions.Count > 0)
					{
						var hint = new BeamHintHeader(BeamHintType.Validation,
													  BeamHintDomains.BEAM_CSHARP_MICROSERVICES_CODE_MISUSE,
													  BeamHintIds.ID_MICROSERVICE_NAME_COLLISION);
						_hintStorage.AddOrReplaceHint(hint, uniqueNameValidationResults.PerNameCollisions);
					}

					// Get all ClientCallables
					var clientCallableValidationResults = cachedMicroserviceAttributes
														  .SelectMany(
															  pair => pair.InfoAs<Type>()
																		  .GetMethods(
																			  BindingFlags.Public |
																			  BindingFlags.Instance))
														  .GetOptionalAttributeInMembers<ClientCallableAttribute>();

					// Handle invalid signatures and warnings
					clientCallableValidationResults.SplitValidationResults(out var clientCallablesValid,
																		   out var clientCallableWarnings,
																		   out var clientCallableErrors);
					if (clientCallableWarnings.Count > 0)
					{
						var hint = new BeamHintHeader(BeamHintType.Hint,
													  BeamHintDomains.BEAM_CSHARP_MICROSERVICES_CODE_MISUSE,
													  BeamHintIds.ID_CLIENT_CALLABLE_ASYNC_VOID);
						_hintStorage.AddOrReplaceHint(hint, clientCallableWarnings);
					}

					if (clientCallableErrors.Count > 0)
					{
						var hint = new BeamHintHeader(BeamHintType.Validation,
													  BeamHintDomains.BEAM_CSHARP_MICROSERVICES_CODE_MISUSE,
													  BeamHintIds.ID_CLIENT_CALLABLE_UNSUPPORTED_PARAMETERS);
						_hintStorage.AddOrReplaceHint(hint, clientCallableErrors);
					}

					// Builds a lookup of DeclaringType => MemberAttribute.
					var validClientCallablesLookup = clientCallablesValid
													 .Concat(clientCallableWarnings)
													 .Concat(clientCallableErrors)
													 .Select(result => result.Pair)
													 .CreateMemberAttributeOwnerLookupTable();

					// Register all configured microservices
					for (int k = 0; k < uniqueNameValidationResults.PerAttributeNameValidations.Count; k++)
					{
						var serviceAttribute = uniqueNameValidationResults.PerAttributeNameValidations[k].Pair
																		  .AttrAs<MicroserviceAttribute>();
						var type = uniqueNameValidationResults.PerAttributeNameValidations[k].Pair.InfoAs<Type>();

						// TODO: XXX this is a hacky way to ignore the default microservice...
						if (serviceAttribute.MicroserviceName.ToLower().Equals("xxxx")) continue;

						if (!File.Exists(serviceAttribute.GetSourcePath()))
							continue;

						// Create descriptor
						var hasWarning = uniqueNameValidationResults.PerAttributeNameValidations[k].Type ==
										 ReflectionCache.ValidationResultType.Warning;
						var hasError = uniqueNameValidationResults.PerAttributeNameValidations[k].Type ==
									   ReflectionCache.ValidationResultType.Error;
						var descriptor = new MicroserviceDescriptor
						{
							Name = serviceAttribute.MicroserviceName,
							Type = type,
							AttributePath = serviceAttribute.GetSourcePath(),
							HasValidationError = hasError,
							HasValidationWarning = hasWarning,
						};

						// Initialize the ClientCallableDescriptors if the type has any.
						if (validClientCallablesLookup.TryGetValue(type, out var clientCallables))
						{
							// Generates descriptors for each of the individual client callables.
							descriptor.Methods = clientCallables.Select(delegate (MemberAttribute pair)
							{
								var clientCallableAttribute = pair.AttrAs<ClientCallableAttribute>();
								var clientCallableMethod = pair.InfoAs<MethodInfo>();

								var callableName = pair.GetOptionalNameOrMemberName<ClientCallableAttribute>();
								var callableScopes = clientCallableAttribute.RequiredScopes;

								var parameters = clientCallableMethod
												 .GetParameters()
												 .Select((param, i) =>
												 {
													 var paramAttr = param.GetCustomAttribute<ParameterAttribute>();
													 var paramName =
														 string.IsNullOrEmpty(paramAttr?.ParameterNameOverride)
															 ? param.Name
															 : paramAttr.ParameterNameOverride;
													 return new ClientCallableParameterDescriptor
													 {
														 Name = paramName,
														 Index = i,
														 Type = param.ParameterType
													 };
												 }).ToArray();

								return new ClientCallableDescriptor()
								{
									Path = callableName,
									Scopes = callableScopes,
									Parameters = parameters,
								};
							}).ToList();
						}
						else // If no client callables were found in the C#MS, initialize an empty list.
						{
							descriptor.Methods = new List<ClientCallableDescriptor>();
						}

						// Check if MS is used for external identity federation
						var interfaces = descriptor.Type.GetInterfaces();

						foreach (var it in interfaces)
						{
							if (!it.IsGenericType) continue;
							if (it.GetGenericTypeDefinition() != typeof(IFederatedLogin<>)) continue;

							var map = descriptor.Type.GetInterfaceMap(it);
							var federatedType = it.GetGenericArguments()[0];

							if (Activator.CreateInstance(federatedType) is IThirdPartyCloudIdentity identity)
							{
								descriptor.FederatedNamespaces.Add(identity.UniqueName);
							}
						}

						Descriptors.Add(descriptor);
						AllDescriptors.Add(descriptor);
					}
				}

				void ParseStorageObjectAttributes(IReadOnlyList<MemberAttribute> cachedStorageObjectAttributes)
				{
					// Searches for all unique name collisions.
					var uniqueNameValidationResults = cachedStorageObjectAttributes
						.GetAndValidateUniqueNamingAttributes<StorageObjectAttribute>();

					// Registers a hint with all name collisions found.
					if (uniqueNameValidationResults.PerNameCollisions.Count > 0)
					{
						var hint = new BeamHintHeader(BeamHintType.Validation,
													  BeamHintDomains.BEAM_CSHARP_MICROSERVICES_CODE_MISUSE,
													  BeamHintIds.ID_STORAGE_OBJECT_NAME_COLLISION);
						_hintStorage.AddOrReplaceHint(hint, uniqueNameValidationResults.PerNameCollisions);
					}

					// Register all configured storage object
					foreach (var storageObjectValResults in uniqueNameValidationResults.PerAttributeNameValidations)
					{
						var serviceAttribute = storageObjectValResults.Pair.AttrAs<StorageObjectAttribute>();
						var type = storageObjectValResults.Pair.InfoAs<Type>();

						// TODO: XXX this is a hacky way to ignore the default microservice...
						if (serviceAttribute.StorageName.ToLower().Equals("xxxx")) continue;

						if (!File.Exists(serviceAttribute.SourcePath))
							continue;

						// Create descriptor
						var hasWarning = storageObjectValResults.Type == ReflectionCache.ValidationResultType.Warning;
						var hasError = storageObjectValResults.Type == ReflectionCache.ValidationResultType.Error;
						var descriptor = new StorageObjectDescriptor
						{
							Name = serviceAttribute.StorageName,
							Type = type,
							AttributePath = serviceAttribute.SourcePath,
							HasValidationWarning = hasWarning,
							HasValidationError = hasError,
						};

						StorageDescriptors.Add(descriptor);
						AllDescriptors.Add(descriptor);
					}
				}
			}

			#region Service Deployment

			public const string SERVICE_PUBLISHED_KEY = "service_published_{0}";

			public event Action<ManifestModel, int> OnBeforeDeploy;
			public event Action<ManifestModel, int> OnDeploySuccess;
			public event Action<ManifestModel, string> OnDeployFailed;
			public event Action<IDescriptor, ServicePublishState> OnServiceDeployStatusChanged;
			public event Action<IDescriptor> OnServiceDeployProgress;

			public async Task Deploy(ManifestModel model,
									 CancellationToken token,
									 Action<IDescriptor> onServiceDeployed = null,
									 Action<LogMessage> logger = null)
			{
				try
				{
					AssetDatabase.StartAssetEditing();
					await DeployInternal(model, token, onServiceDeployed, logger);
				}
				finally
				{
					AssetDatabase.StopAssetEditing();
				}
			}

			private async Task DeployInternal(ManifestModel model,
											  CancellationToken token,
											  Action<IDescriptor> onServiceDeployed = null,
											  Action<LogMessage> logger = null)
			{
				// if (Descriptors.Count == 0) return; // don't do anything if there are no descriptors.

				if (logger == null)
				{
					logger = message => Debug.Log($"[{message.Level}] {message.Timestamp} - {message.Message}");
				}

				var descriptorsCount = Descriptors.Count;

				OnBeforeDeploy?.Invoke(model, descriptorsCount);

				OnDeploySuccess -= HandleDeploySuccess;
				OnDeploySuccess += HandleDeploySuccess;
				OnDeployFailed -= HandleDeployFailed;
				OnDeployFailed += HandleDeployFailed;

				// TODO perform sort of diff, and only do what is required. Because this is a lot of work.
				var de = BeamEditorContext.Default;
				await de.InitializePromise;

				var client = de.GetMicroserviceManager();
				var existingManifest = await client.GetCurrentManifest();
				var secret = await de.GetRealmSecret();
				var existingServiceToState = existingManifest.manifest.ToDictionary(s => s.serviceName);

				var nameToImageDetails = new Dictionary<string, ImageDetails>();
				var enabledServices = new List<string>();

				// if any service's storage dependencies are archived, and the service is also not archived, we are in trouble...
				foreach (var service in model.Services)
				{
					var anyArchivedStorages = service.Value.Dependencies.Any(d =>
					{
						if (!model.Storages.TryGetValue(d.id, out var storage))
						{
							return true;
						}

						return storage.Archived;
					});
					if (anyArchivedStorages && !service.Value.Archived)
					{
						var msg =
							$"Cannot deploy, because {service.Key} depends on an archived storage. Either unarchive the storage, or archive the service.";
						logger(new LogMessage
						{
							Level = LogLevel.FATAL,
							Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
							Message = msg
						});
						OnDeployFailed?.Invoke(model, msg);
						return;
					}
				}

				var availableArchitectures = await new GetBuildOutputArchitectureCommand().StartAsync();

				foreach (var descriptor in Descriptors)
				{
					UpdateServiceDeployStatus(descriptor, ServicePublishState.InProgress);

					logger(new LogMessage
					{
						Level = LogLevel.INFO,
						Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
						Message = $"Checking service=[{descriptor.Name}]"
					});

					var entryModel = model.Services[descriptor.Name];

					// If the _local_ service is archived, then we don't need to bother doing anything else here...
					if (entryModel.Archived)
					{
						logger(new LogMessage
						{
							Level = LogLevel.INFO,
							Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
							Message = $"Skipping service=[{descriptor.Name}] because it is archived."
						});
						continue;
					}

					// If the service is disabled, then we won't bother uploading it.
					if (!entryModel.Enabled)
					{
						logger(new LogMessage
						{
							Level = LogLevel.INFO,
							Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
							Message = $"Skipping service=[{descriptor.Name}] because it is disabled."
						});
						UpdateServiceDeployStatus(descriptor, ServicePublishState.Published);
						onServiceDeployed?.Invoke(descriptor);
						continue;
					}

					var forceStop = new StopImageReturnableCommand(descriptor);
					await forceStop.StartAsync(); // force the image to stop.
					await BeamServicesCodeWatcher
						.StopClientSourceCodeGenerator(descriptor); // force the generator to stop.

					// Build the image
					try
					{
						var buildCommand = new BuildImageCommand(descriptor, availableArchitectures,
																 includeDebugTools: false,
																 watch: false,
																 pull: true,
																 cpuContext: CPUArchitectureContext.DEPLOY);

						await buildCommand.StartAsync();
					}
					catch (Exception e)
					{
						OnDeployFailed?.Invoke(model, $"Deploy failed due to {descriptor.Name} failing to build: {e}.");
						UpdateServiceDeployStatus(descriptor, ServicePublishState.Failed);

						return;
					}

					// Try to start the image and talk to it's healthcheck endpoint.
					try
					{
						async Promise<string> CheckHealthStatus()
						{
							var comm = new DockerPortCommand(descriptor, Constants.Features.Services.HEALTH_PORT);
							var dockerPortResult = await comm.StartAsync();

							if (!dockerPortResult.ContainerExists)
								return "false";

							var res = await de.ServiceScope.GetService<IEditorHttpRequester>()
											  .ManualRequest<string>(
												  Method.GET, $"http://{dockerPortResult.LocalFullAddress}/health",
												  parser: x => x);
							return res;
						}

						if (MicroserviceConfiguration.Instance.EnablePrePublishHealthCheck)
						{
							// We are now verifying the image we just built
							UpdateServiceDeployStatus(descriptor, ServicePublishState.Verifying);

							// Check to see if the storage descriptor is running.
							var connectionStrings =
								await GetConnectionStringEnvironmentVariables((MicroserviceDescriptor)descriptor);

							// Create a build that will build an image that doesn't run the custom initialization hooks
							// Let's run it locally.
							// At the moment we disable running custom hooks for this verification.
							// This is because we cannot guarantee the user won't do anything in them to break this.
							// TODO: Change algorithm to always have StorageObjects running locally during verification process.
							// TODO: Allow users to enable running custom hooks on specific C#MSs instances --- this implies they'd know what they are doing.
							var runServiceCommand = new RunServiceCommand(
								descriptor, de.CurrentCustomer.Cid, de.CurrentRealm.Pid, secret, connectionStrings,
								false, false);
							runServiceCommand.Start();

							var healthcheckTimeout =
								MicroserviceConfiguration.Instance.PrePublishHealthCheckTimeout.GetOrElse(10);

							// Wait until the container has completely booted up and it's Start function has finished.
							var timeWaitingForBoot = 0f;
							var isHealthy = false;
							do
							{
								try
								{
									var healthStatus = await CheckHealthStatus();
									if (healthStatus.Contains("true"))
										isHealthy = true;

									if (healthStatus.Contains("false"))
										isHealthy = false;
								}
								catch
								{
									isHealthy = false;
								}

								await Task.Delay(500, token);
								timeWaitingForBoot += .5f;
							} while (timeWaitingForBoot <= healthcheckTimeout && !isHealthy);

							if (!isHealthy)
							{
								OnDeployFailed?.Invoke(
									model,
									$"Deploy failed due to build of {descriptor.Name} failing to start. Check out the C#MS logs to understand why.");
								UpdateServiceDeployStatus(descriptor, ServicePublishState.Failed);

								// Stop the container since we don't need to keep the local one alive anymore.
								await new StopImageCommand(descriptor).StartAsync();

								return;
							}
						}

						// Stop the container since we don't need to keep the local one alive anymore.
						await new StopImageCommand(descriptor).StartAsync();
					}
					catch (Exception e)
					{
						OnDeployFailed?.Invoke(
							model,
							$"Deploy failed due to build of {descriptor.Name} failing to start: Exception={e} Message={e.Message}.");
						UpdateServiceDeployStatus(descriptor, ServicePublishState.Failed);

						return;
					}

					UpdateServiceDeployStatus(descriptor, ServicePublishState.InProgress);

					if (token.IsCancellationRequested)
					{
						OnDeployFailed?.Invoke(model, $"Cancellation requested after build of {descriptor.Name}.");
						UpdateServiceDeployStatus(descriptor, ServicePublishState.Failed);

						return;
					}

					var uploader = new ContainerUploadHarness();
					var msModel = MicroservicesDataModel.Instance.GetModel<MicroserviceModel>(descriptor);
					uploader.onProgress += msModel.OnDeployProgress;
					uploader.onProgress += (_, __, ___) => OnServiceDeployProgress?.Invoke(descriptor);

					logger(new LogMessage
					{
						Level = LogLevel.INFO,
						Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
						Message = $"Getting Id service=[{descriptor.Name}]"
					});

					var imageDetails = await uploader.GetImageId(descriptor);
					var imageId = imageDetails.imageId;
					if (string.IsNullOrEmpty(imageId))
					{
						OnDeployFailed?.Invoke(
							model,
							$"Failed due to failed Docker {nameof(GetImageDetailsCommand)} for {descriptor.Name}.");
						UpdateServiceDeployStatus(descriptor, ServicePublishState.Failed);
						return;
					}

					// the architecture needs to be one of the supported beamable architectures...
					if (!CPU_SUPPORTED.Any(imageDetails.Platform.Contains))
					{
						OnDeployFailed?.Invoke(
							model,
							$"Beamable cannot accept an image built for {imageDetails.Platform}. Please use one of the following... {string.Join(",", CPU_SUPPORTED)}");
						UpdateServiceDeployStatus(descriptor, ServicePublishState.Failed);
						return;
					}

					nameToImageDetails.Add(descriptor.Name, imageDetails);

					if (existingServiceToState.TryGetValue(descriptor.Name, out var existingReference))
					{
						if (existingReference.imageId == imageId)
						{
							logger(new LogMessage
							{
								Level = LogLevel.INFO,
								Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
								Message = string.Format(CONTAINER_ALREADY_UPLOADED_MESSAGE, descriptor.Name)
							});

							onServiceDeployed?.Invoke(descriptor);
							UpdateServiceDeployStatus(descriptor, ServicePublishState.Published);

							foreach (var storage in descriptor.GetStorageReferences())
							{
								if (!enabledServices.Contains(storage.Name))
									enabledServices.Add(storage.Name);
							}

							continue;
						}
					}

					var serviceDependencies = new List<ServiceDependency>();
					foreach (var storage in descriptor.GetStorageReferences())
					{
						if (!enabledServices.Contains(storage.Name))
							enabledServices.Add(storage.Name);

						serviceDependencies.Add(new ServiceDependency { id = storage.Name, storageType = "storage" });
					}

					entryModel.Dependencies = serviceDependencies;

					logger(new LogMessage
					{
						Level = LogLevel.INFO,
						Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
						Message = $"Uploading container service=[{descriptor.Name}]"
					});
					await uploader.UploadContainer(descriptor, token, () =>
												   {
													   Debug.Log(string.Format(UPLOAD_CONTAINER_MESSAGE,
																			   descriptor.Name));
													   onServiceDeployed?.Invoke(descriptor);
													   UpdateServiceDeployStatus(
														   descriptor, ServicePublishState.Published);
												   },
												   () =>
												   {
													   Debug.LogError(
														   string.Format(
															   CANT_UPLOAD_CONTAINER_MESSAGE, descriptor.Name));
													   if (token.IsCancellationRequested)
													   {
														   OnDeployFailed?.Invoke(
															   model,
															   $"Cancellation requested during upload of {descriptor.Name}.");
														   UpdateServiceDeployStatus(
															   descriptor, ServicePublishState.Failed);
													   }
												   }, imageId);
				}

				// at this point, all storage objects should at least be marked as complete.
				foreach (var storage in MicroserviceConfiguration.Instance.StorageObjects)
				{
					logger(new LogMessage
					{
						Level = LogLevel.INFO,
						Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
						Message = $"Comitting storage=[{storage.StorageName}]"
					});
					var storageDesc = new StorageObjectDescriptor { Name = storage.StorageName };
					onServiceDeployed?.Invoke(storageDesc);
					OnServiceDeployStatusChanged?.Invoke(storageDesc, ServicePublishState.Published);
				}

				// we should mark all remote services as complete as well.
				var remoteOnlyServices = model.Services.Where(s => !nameToImageDetails.ContainsKey(s.Key)).ToList();
				foreach (var remoteOnly in remoteOnlyServices)
				{
					var desc = new MicroserviceDescriptor { Name = remoteOnly.Key };
					onServiceDeployed?.Invoke(desc);
					OnServiceDeployStatusChanged?.Invoke(desc, ServicePublishState.Published);
				}

				logger(new LogMessage
				{
					Level = LogLevel.INFO,
					Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
					Message = $"Deploying Manifest..."
				});

				// Manifest Building:
				// 1- Find all locally know services and build their references (using the latest uploaded image ids for them).
				var localServiceReferences = nameToImageDetails.Select(kvp =>
				{
					var sa = model.Services[kvp.Key];
					return new ServiceReference()
					{
						serviceName = sa.Name,
						templateId = sa.TemplateId,
						enabled = sa.Enabled,
						archived = sa.Archived,
						comments = sa.Comment,
						imageId = kvp.Value.imageId,
						imageCpuArch = kvp.Value.cpuArch,
						dependencies = sa.Dependencies
					};
				}).Where(service => !service
							 .archived); // If this is a local-only service, and its archived, best not to send it _at all_.

				// 2- Finds all Known Remote Service (and their last uploaded configuration/image id).
				var remoteServiceReferences = model.ServerManifest.Select(kvp => kvp.Value);

				// 3- Join those two lists and configure the enabled status of all services based on the user input (stored in model.Services[serviceName].Enabled)
				var manifest = localServiceReferences.Union(remoteServiceReferences).ToList();
				foreach (var uploadServiceReference in manifest)
				{
					var sa = model.Services[uploadServiceReference.serviceName];
					uploadServiceReference.enabled = sa.Enabled;
					uploadServiceReference.templateId = sa.TemplateId;
					uploadServiceReference.archived = sa.Archived;
				}

				// 4- Make sure we only have each service once on the list.
				manifest = manifest.Distinct(new ServiceReferenceNameComp()).ToList();

				// Identify storages to enable
				// 1- Make a list of all dependencies that are depended on by any of the services that will be enabled
				var allDependenciesThatMustBeEnabled = manifest
													   .Where(serviceRef => serviceRef.enabled)
													   .SelectMany(sr => sr.dependencies)
													   .Select(deps => deps.id)
													   .ToList();

				// 2- Only enable storages that are actually depended on by services.
				var storageManifest = model.Storages.Select(kvp =>
				{
					kvp.Value.Enabled &= allDependenciesThatMustBeEnabled.Contains(kvp.Value.Name);
					return new ServiceStorageReference
					{
						id = kvp.Value.Name,
						storageType = kvp.Value.Type,
						templateId = kvp.Value.TemplateId,
						enabled = kvp.Value.Enabled,
						archived = kvp.Value.Archived
					};
				}).ToList();

				await client.Deploy(new ServiceManifest
				{
					comments = model.Comment,
					manifest = manifest,
					storageReference = storageManifest
				});
				OnDeploySuccess?.Invoke(model, descriptorsCount);

				logger(new LogMessage
				{
					Level = LogLevel.INFO,
					Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
					Message = $"Service Deploy Complete"
				});

				void HandleDeploySuccess(ManifestModel _, int __)
				{
					WindowStateUtility.EnableAllWindows();
				}

				void HandleDeployFailed(ManifestModel _, string __)
				{
					WindowStateUtility.EnableAllWindows();
				}
			}

			private struct ServiceReferenceNameComp : IEqualityComparer<ServiceReference>
			{
				public bool Equals(ServiceReference x, ServiceReference y)
				{
					if (ReferenceEquals(x, y))
					{
						return true;
					}

					if (ReferenceEquals(x, null))
					{
						return false;
					}

					if (ReferenceEquals(y, null))
					{
						return false;
					}

					if (x.GetType() != y.GetType())
					{
						return false;
					}

					return x.serviceName == y.serviceName;
				}

				public int GetHashCode(ServiceReference obj)
				{
					return (obj.serviceName != null ? obj.serviceName.GetHashCode() : 0);
				}
			}

			public void MicroserviceCreated(string serviceName)
			{
				var key = string.Format(SERVICE_PUBLISHED_KEY, serviceName);
				EditorPrefs.SetBool(key, false);
			}

			public Promise<ManifestModel> GenerateUploadModel()
			{
				// first, get the server manifest
				var de = BeamEditorContext.Default;
				var client = de.GetMicroserviceManager();
				return client.GetCurrentManifest().Map(manifest =>
				{
					var allServices = new HashSet<string>();

					// make sure all server-side things are represented
					foreach (var serverSideService in manifest.manifest.Select(s => s.serviceName))
					{
						allServices.Add(serverSideService);
					}

					// add in anything locally...
					foreach (var descriptor in Descriptors)
					{
						allServices.Add(descriptor.Name);
					}

					// get enablement for each service...
					var entries = allServices.Select(name =>
					{
						var configEntry =
							MicroserviceConfiguration.Instance
													 .GetEntry(
														 name); //config.FirstOrDefault(s => s.ServiceName == name);
						var descriptor = Descriptors.FirstOrDefault(d => d.Name == configEntry.ServiceName);
						var remote = manifest.manifest.FirstOrDefault(s => string.Equals(s.serviceName, name));
						var serviceDependencies = new List<ServiceDependency>();
						if (descriptor != null)
						{
							foreach (var storage in descriptor.GetStorageReferences())
							{
								serviceDependencies.Add(
									new ServiceDependency { id = storage.Name, storageType = "storage" });
							}
						}
						else if (remote != null)
						{
							// this is a remote service, and we should keep its references intact...
							serviceDependencies.AddRange(remote.dependencies);
						}

						return new ManifestEntryModel
						{
							Comment = "",
							Name = name,
							Enabled = configEntry?.Enabled ?? true,
							Archived = configEntry?.Archived ?? false,
							TemplateId = configEntry?.TemplateId ?? "small",
							Dependencies = serviceDependencies
						};
					}).ToList();

					var allStorages = new HashSet<string>();

					foreach (var serverSideStorage in manifest.storageReference.Select(s => s.id))
					{
						allStorages.Add(serverSideStorage);
					}

					foreach (var storageDescriptor in StorageDescriptors)
					{
						allStorages.Add(storageDescriptor.Name);
					}

					var storageEntries = allStorages.Select(name =>
					{
						var configEntry = MicroserviceConfiguration.Instance.GetStorageEntry(name);
						return new StorageEntryModel
						{
							Name = name,
							Type = configEntry?.StorageType ?? "mongov1",
							Enabled = configEntry?.Enabled ?? true,
							Archived = configEntry?.Archived ?? false,
							TemplateId = configEntry?.TemplateId ?? "small",
						};
					}).ToList();

					return new ManifestModel
					{
						ServerManifest = manifest.manifest.ToDictionary(e => e.serviceName),
						Comment = "",
						Services = entries.ToDictionary(e => e.Name),
						Storages = storageEntries.ToDictionary(s => s.Name)
					};
				});
			}

			private void UpdateServiceDeployStatus(MicroserviceDescriptor descriptor, ServicePublishState status)
			{
				OnServiceDeployStatusChanged?.Invoke(descriptor, status);

				foreach (var storageDesc in descriptor.GetStorageReferences())
					OnServiceDeployStatusChanged?.Invoke(storageDesc, status);
			}

			#endregion

			#region Running Services

			public async Promise<Dictionary<string, string>> GetConnectionStringEnvironmentVariables(
				MicroserviceDescriptor service)
			{
				var env = new Dictionary<string, string>();
				foreach (var reference in service.GetStorageReferences())
				{
					var key = $"STORAGE_CONNSTR_{reference.Name}";
					env[key] = await GetConnectionString(reference);
				}

				return env;
			}

			public async Promise<string> GetConnectionString(StorageObjectDescriptor storage)
			{
				var storageCheck = new CheckImageReturnableCommand(storage);
				var isStorageRunning = await storageCheck.StartAsync();
				if (isStorageRunning)
				{
					var config = MicroserviceConfiguration.Instance.GetStorageEntry(storage.Name);
					return
						$"mongodb://{config.LocalInitUser}:{config.LocalInitPass}@gateway.docker.internal:{config.LocalDataPort}";
				}
				else
				{
					return "";
				}
			}

			#endregion

			#region Service Builders

			public MicroserviceBuilder GetServiceBuilder(MicroserviceDescriptor descriptor)
			{
				var key = descriptor.Name;
				if (!_serviceToBuilder.ContainsKey(key))
				{
					var builder = new MicroserviceBuilder();
					builder.Init(descriptor);
					_serviceToBuilder.Add(key, builder);
				}

				return _serviceToBuilder[key];
			}

			public MongoStorageBuilder GetStorageBuilder(StorageObjectDescriptor descriptor)
			{
				var key = descriptor.Name;

				if (_storageToBuilder.ContainsKey(key))
					return _storageToBuilder[key];

				var builder = new MongoStorageBuilder();
				builder.Init(descriptor);
				_storageToBuilder.Add(key, builder);
				return _storageToBuilder[key];
			}

			#endregion
		}
	}
}
