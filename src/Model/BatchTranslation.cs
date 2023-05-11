using System.Collections.Generic;

namespace ModernMT.Model
{
    public class BatchTranslation : Model
    {
        private readonly dynamic _data;
        
        public Translation Data => _data as Translation;
        
        public List<Translation> DataList
        {
            get
            {
                if (_data is List<Translation> list)
                    return list;
        
                return new List<Translation>{_data};
            }
        
        }

        public readonly dynamic Metadata;

        public BatchTranslation(dynamic data, dynamic metadata)
        {
            _data = data;
            Metadata = metadata;
        }

    }
}