using System.Collections.Generic;

namespace Framewerk.Examples.ListExample
{
    public class ContactDataProvider
    {
        public List<ContactData> GetContactData()
        {
            return new List<ContactData>
            {
                new ContactData { Name = "Alice Johnson", Phone = "+1 555-0101" },
                new ContactData { Name = "Bob Smith", Phone = "+1 555-0102" },
                new ContactData { Name = "Carol Davis", Phone = "+1 555-0103" },
                new ContactData { Name = "David Wilson", Phone = "+1 555-0104" },
                new ContactData { Name = "Eve Brown", Phone = "+1 555-0105" }
            };
        }
    }
}
