using BobTelegramBot.ServiceHelper;
using Containers;
using Branch = BobTelegramBot.ServiceHelper.Branch;

namespace BobTelegramBot.Structures
{
    static class LocationHelper
    {
        public static LocationItem FromBranch(Branch item)
        {
            return new LocationItem(
                                    item.Id,
                                    ToInternalMultistring(item.Title),
                                    ToInternalMultistring(item.Desc),
                                    item.Latitude,
                                    item.Longitude);
        }

        public static LocationItem FromAtm(Atm item)
        {
            return new LocationItem(
                                    item.Id,
                                    ToInternalMultistring(item.Title),
                                    ToInternalMultistring(item.Desc),
                                    item.Latitude,
                                    item.Longitude);
        }

        public static LocationItem FromCashIn(PaymentTerminal item)
        {
            return new LocationItem(
                                    item.Id,
                                    ToInternalMultistring(item.Title),
                                    ToInternalMultistring(item.Desc),
                                    item.Latitude,
                                    item.Longitude);
        }

        private static Containers.MultiLanguageString ToInternalMultistring(
            ServiceHelper.MultiLanguageString multiStr)
        {
            if (multiStr == null)
            {
                return new Containers.MultiLanguageString();
            }

            return new Containers.MultiLanguageString(
                                                      multiStr.Az,
                                                      multiStr.En,
                                                      multiStr.Ru
                                                     );
        }
    }
}
