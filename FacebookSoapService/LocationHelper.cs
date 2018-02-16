using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Containers;
using FacebookSoapService.ServiceHelper;
using Branch = FacebookSoapService.ServiceHelper.Branch;

namespace FacebookSoapService
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
