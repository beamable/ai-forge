using Beamable.Common.Api;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Content;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

namespace Beamable.Platform.Tests.Inventory.InventoryServiceTests
{
	public class UpdateTests : InventoryServiceTestBase
	{
		//      [UnityTest]
		//      public IEnumerator TestDeleteItemWithUpdateBuilder()
		//      {
		//         var updateBuilder = new InventoryUpdateBuilder
		//         {
		//            deleteItems =
		//            {
		//               new ItemDeleteRequest
		//               {
		//                  contentId = "items.tunatruck",
		//                  itemId = 123
		//               }
		//            }
		//         };
		//
		////         _service.Update(updateBuilder);
		//      }

		[UnityTest]
		public IEnumerator TestAddItemWithUpdateBuilder()
		{
			var props = new Dictionary<string, string>
		 {
			{"a", "b"}
		 };
			var updateBuilder = new InventoryUpdateBuilder
			{
				newItems = {
			   new ItemCreateRequest
			   {
				  contentId = InventoryTestItem.FULL_CONTENT_ID,
				  properties = props.ToSerializable()
			   }
			}
			};

			// stub out request.
			_requester.MockRequest<EmptyResponse>(Method.PUT, null)
					  .WithURIPrefix("/object/inventory")
			   .WithoutJsonField("transaction")
			   .WithJsonFieldMatch("newItems[0].contentId", obj =>
			   {
				   if (obj is string contentId)
				   {
					   return contentId.Equals(InventoryTestItem.FULL_CONTENT_ID);
				   }
				   return false;
			   })
			   .WithoutJsonField("currencies")
			   .WithoutJsonField("updateItems")
			   .WithoutJsonField("deleteItems")
			   ;


			yield return _service.Update(updateBuilder).Then(_ =>
			{
				// asserts may go here.
			}).AsYield();

		}
	}
}
