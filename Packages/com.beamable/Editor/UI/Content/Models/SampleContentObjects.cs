using Beamable.Common.Content;
using Beamable.Common.Inventory;

#if BEAMABLE_DEVELOPER
namespace Beamable.Editor.Content.Models
{
   [ContentType("weapons")]
   public class WeaponItem : ItemContent
   {
      public int damage;
      public int attackSpeed;
   }

   [ContentType("melee")]
   public class MeleeWeapon : WeaponItem
   {
      public bool twoHanded;
   }

   [ContentType("ranged")]
   public class RangedWeapon : WeaponItem
   {
      public int range;
   }

   [ContentType("trinkets")]
   public class TrinketItem : ItemContent
   {
      public int aura;
   }
}
#endif
