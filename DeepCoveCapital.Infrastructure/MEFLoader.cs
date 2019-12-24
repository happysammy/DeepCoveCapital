using System;
using System.Collections.Generic;
using System.Composition;

namespace DeepCoveCapital.Infrastructure
{
    public class MEFLoader
    {//TODO
      /*  private CompositionContainer _Container;
        private string _repositoryConfigurationType;

        public MEFLoader(string path = "TradingApp")
        {
            //DirectoryCatalog directoryCatalog = new DirectoryCatalog(path);
            AssemblyCatalog assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());

            //An aggregate catalog that combines multiple catalogs 
            var catalog = new AggregateCatalog(assemblyCatalog);

            // Create the CompositionContainer with all parts in the catalog (links Exports and Imports) 
            _Container = new CompositionContainer(catalog);

            //Fill the imports of this object 
            _Container.ComposeParts(this);

            //read configuration type for repositories
            _repositoryConfigurationType = ConfigurationManager.AppSettings["RepositoryConfigurationType"];
        }

        #region composable parts
        [ImportMany]
        public IEnumerable<IExchange> ExchangeClients { get; set; }

        [ImportMany]
        IEnumerable<Lazy<ICandleStickRepository, IRepositoryMetadata>> CandleStickRepositories { get; set; }

        [ImportMany]
        IEnumerable<Lazy<IConfirmationRepository, IRepositoryMetadata>> ConfirmationRepositories { get; set; }

        [ImportMany]
        IEnumerable<Lazy<IOrderRepository, IRepositoryMetadata>> OrderRepositories { get; set; }

        [ImportMany]
        IEnumerable<Lazy<ISetupRepository, IRepositoryMetadata>> SetupRepositories { get; set; }
        #endregion

        #region public repositories
        ICandleStickRepository candleStickRepository;
        public ICandleStickRepository CandleStickRepository
        {
            get
            {
                if (candleStickRepository == null)
                    candleStickRepository = this.CandleStickRepositories.FirstOrDefault(r => r.Metadata.Nature.Equals(_repositoryConfigurationType)).Value;
                return candleStickRepository;
            }
        }

        IConfirmationRepository confirmationRepository;
        public IConfirmationRepository ConfirmationRepository
        {
            get
            {
                if (confirmationRepository == null)
                    confirmationRepository = this.ConfirmationRepositories.FirstOrDefault(r => r.Metadata.Nature.Equals(_repositoryConfigurationType)).Value;
                return confirmationRepository;
            }
        }

        ISetupRepository setupRepository;
        public ISetupRepository SetupRepository
        {
            get
            {
                if (setupRepository == null)
                    setupRepository = this.SetupRepositories.FirstOrDefault(r => r.Metadata.Nature.Equals(_repositoryConfigurationType)).Value;
                return setupRepository;
            }
        }

        IOrderRepository orderRepository;
        public IOrderRepository OrderRepository
        {
            get
            {
                if (orderRepository == null)
                    orderRepository = this.OrderRepositories.FirstOrDefault(r => r.Metadata.Nature.Equals(_repositoryConfigurationType)).Value;
                return orderRepository;
            }
        }
        #endregion
    */
    }
}
