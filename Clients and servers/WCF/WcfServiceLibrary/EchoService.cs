using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfServiceLibrary
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "EchoService" в коде и файле конфигурации.
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class EchoService : IEchoService
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
               //composite.StringValue += "Suffix";
                composite.StringValue+= composite.IntValue;
            }
            return composite;
        }
    }
}
