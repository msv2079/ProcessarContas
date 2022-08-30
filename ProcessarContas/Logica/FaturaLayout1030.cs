using ProcessarContas.Dados;
using ProcessarContas.Entidade;
using System.Globalization;
using System.Text;

namespace ProcessarContas.Logica
{
	internal class FaturaLayout1030 : IFatura
	{
		public string GetExtensaoArquivo()
		{
			return "csv";
		}

		public bool Processar(string fileName, int nrRegistrosProcessar)
		{
			var resultado = false;

			try
			{
				if (File.Exists(fileName))
				{
					var contLinhas = 0;
					var listaFaturas = new List<FaturaEntidade>();
					var dados = new FaturaDados();
					const Int32 BufferSize = 128;

					using (var fileStream = File.OpenRead(fileName))
					using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
					{
						String line;
						while ((line = streamReader.ReadLine()) != null)
						{
							var splitData = line.Split(";");

							listaFaturas.Add(new FaturaEntidade
							{
								Descricao = splitData[0],
								DataVencimento = DateTime.ParseExact(splitData[1], "yyyyMMdd", CultureInfo.InvariantCulture),
								Valor = Convert.ToDouble(splitData[2])
							});

							contLinhas++;

							if (contLinhas >= nrRegistrosProcessar)
							{
								dados.GravarDados(listaFaturas);

								contLinhas = 0;
								listaFaturas.Clear();
							}
						}

						if (listaFaturas.Count > 0)
						{
							dados.GravarDados(listaFaturas);
							listaFaturas.Clear();
						}
					}

					resultado = true;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return resultado;
		}
	}
}
