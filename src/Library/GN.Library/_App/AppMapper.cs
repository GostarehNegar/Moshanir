using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using AutoMapper.Configuration;

namespace GN.Library
{
    public interface IAppMapper
    {
        IMappingExpression<T1, T2> CreateMap<T1, T2>();
        /// <summary>
        /// Maps an instance of T1, to T2.
        /// It automatically creates a new map, if map is not
        /// alreay defined.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        T2 Map<T1, T2>(T1 value);
        void Clear();


    }
    class AppMapper : IAppMapper
    {
        public static AppMapper Instance = new AppMapper();
        private MapperConfigurationExpression mappings = new MapperConfigurationExpression() { CreateMissingTypeMaps = true };
        private MapperConfiguration configuration;
        private IMapper mapper;
        public AppMapper()
        {
            Clear();
        }
        private IMapper GetMapper(bool refresh = false)
        {
            if (mapper == null || refresh)
                mapper = new Mapper(new MapperConfiguration(mappings));
            return mapper;
        }
        public IMappingExpression<T1, T2> CreateMap<T1, T2>()
        {
            var result = mappings.CreateMap<T1, T2>();
            
            mapper = null;
            return result;
        }
        public T2 Map<T1, T2>(T1 value)
        {
            return GetMapper().Map<T1, T2>(value);
        }

        public void Clear()
        {
            this.mappings = new MapperConfigurationExpression
            {
                CreateMissingTypeMaps = true,
                ValidateInlineMaps = false
            };
            this.configuration = new MapperConfiguration(this.mappings);
            this.mapper = null;
        }
    }
}
