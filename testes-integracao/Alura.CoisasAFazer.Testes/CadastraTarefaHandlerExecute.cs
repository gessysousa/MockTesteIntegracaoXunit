using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Alura.CoisasAFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void DadaTarefaComInformacoesValidasDeveIncluirNoBD()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2022, 02, 19));

            var mock = new Mock<ILogger<CadastraTarefaHandler>>();

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;
            var contexto = new DbTarefasContext(options);

            var repo = new RepositorioTarefa(contexto);

            var handler = new CadastraTarefaHandler(repo, mock.Object);

            //act
            handler.Execute(comando);

            //assert
            var tarefa = repo.ObtemTarefas(t => t.Titulo == "Estudar xUnit").FirstOrDefault();
            Assert.NotNull(tarefa);
        }

        delegate void CapturarMensagemLog(LogLevel level, EventId eventId, object state, Exception exception, Func<object, Exception, string> function);

        [Fact]
        public void DadaTarefaComInfoValidasDeveLogar()
        {
            //arrange
            var tituloTarefaEsperado = "Usar Moq para aprofundar conhecimento de API";
            var comando = new CadastraTarefa(tituloTarefaEsperado, new Categoria("Estudo"), new DateTime(2022, 02, 19));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            LogLevel levelCapturado = LogLevel.Error;
            string mensagemCapturada = string.Empty;

            CapturarMensagemLog captura = (level, eventId, state, exception, func) =>
            {
                levelCapturado = level;
                mensagemCapturada = func(state, exception);
            };

            mockLogger.Setup(l =>
            l.Log(
                It.IsAny<LogLevel>(),      //nível de log => LogError
                It.IsAny<EventId>(), //identificador do evento
                It.IsAny<object>(),  //objeto que será logado
                It.IsAny<Exception>(),     //exceção que será logada
                It.IsAny<Func<object, Exception, string>>() //função que converte objeto + exceção >> string)
            )).Callback(captura); 

            var mock = new Mock<IRepositorioTarefas>();

            var handler = new CadastraTarefaHandler(mock.Object, mockLogger.Object);

            //act
            handler.Execute(comando);

            //assert
            Assert.Equal(LogLevel.Debug, levelCapturado);
            Assert.Contains("Usar Moq para aprofundar conhecimento de API", mensagemCapturada);
        }

        [Fact]
        public void QuandoExceptionForLancadaResultadoIsSuccessDeveSerFalso()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2022, 02, 19));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            var mock = new Mock<IRepositorioTarefas>();
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(new Exception("Houve um erro na inclusão de tarefas")); //quando o método IncluirTarefas for chamado pra qualquer argumento de entrada do tipo Array de tarefas vai lançar uma exception
            var repo = mock.Object;//pega o objetro criado pelo mock e passa como argumento
            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            //act
            CommandResult resultado = handler.Execute(comando);

            //Assert
            Assert.False(resultado.IsSuccess);

        }

        [Fact]
        public void QuandoExceptionForLancadaDeveLogarAMensagemDaExcecao()
        {
            //arrange
            var mensagemDeErroEsperada = "Houve um erro na inclusão de tarefas";
            var excecaoEsperada = new Exception(mensagemDeErroEsperada);
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2022, 02, 19));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>(); 

            var mock = new Mock<IRepositorioTarefas>();
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(excecaoEsperada); //quando o método IncluirTarefas for chamado pra qualquer argumento de entrada do tipo Array de tarefas vai lançar uma exception
            var repo = mock.Object;//pega o objetro criado pelo mock e passa como argumento
            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            //act
            CommandResult resultado = handler.Execute(comando);

            //assert 
            mockLogger.Verify(l => 
            l.Log(
                LogLevel.Error,      //nível de log => LogError
                It.IsAny<EventId>(), //identificador do evento
                It.IsAny<object>(),  //objeto que será logado
                excecaoEsperada,     //exceção que será logada
                It.IsAny<Func<object, Exception, string>>()
            ), //função que converte objeto + exceção >> string
            Times.Once());
        }
    }
}
