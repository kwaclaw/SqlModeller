using System;
using System.Collections.Generic;
using SqlModeller.Interfaces;
using SqlModeller.Model;
using System.Reflection;

namespace SqlModeller.Compiler.SqlServer.Base
{
    public abstract class CompilerRegistry<TValue, TCompiler> where TCompiler : ISqlStatementCompiler<TValue>
    {
        protected List<TCompiler> registeredCompilers;

        protected abstract void Register();

        protected CompilerRegistry()
        {
            Register();
        }

        public string Compile(TValue value, SelectQuery query, IQueryParameterManager parameters)
        {
            var matchingCompiler = FindCompilerForValue(value);

            if (matchingCompiler == null)
            {
                throw new Exception(string.Format("No {0} found for {1}", 
                                        typeof(TCompiler).Name,
                                        value.GetType().Name
                                    ));
            }

            return matchingCompiler.Compile(value, query, parameters);
        }
        
        public TCompiler FindCompilerForValue(TValue filter)
        {
            var filterType = filter.GetType();
             
            foreach (var compiler in registeredCompilers)
            {
                var t = compiler.GetType().GetTypeInfo();

                foreach (Type intType in t.ImplementedInterfaces)
                {
                    var it = intType.GetTypeInfo();
                    if (it.IsGenericType)
                    {
                        //todo  : check if intType : TCompiler

                        if (it.GenericTypeArguments[0] == filterType)
                        {
                            return compiler;
                        } 
                    }  
                }
            }
            return default(TCompiler);
        }
    }

    
}
