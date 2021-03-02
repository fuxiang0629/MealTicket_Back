using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http.Description;

namespace WebAPI.HelpPage.ModelDescriptions
{
    public class ParameterDescription
    {
        public ParameterDescription()
        {
            Annotations = new Collection<ParameterAnnotation>();
        }

        public Collection<ParameterAnnotation> Annotations { get; private set; }

        public string Documentation { get; set; }

        public string Name { get; set; }

        public ModelDescription TypeDescription { get; set; }

        public int Level { get; set; }

        public ApiParameterSource Source { get; set; }
    }
}