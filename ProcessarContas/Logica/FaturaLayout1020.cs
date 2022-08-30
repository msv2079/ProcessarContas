using ProcessarContas.Dados;
using ProcessarContas.Entidade;
using System.Globalization;
using System.Text;
using System.Xml;

namespace ProcessarContas.Logica
{
	internal class FaturaLayout1020 : IFatura
	{
		public string GetExtensaoArquivo()
		{
			return "xml";
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

					XmlDocument doc = new XmlDocument();
					doc.Load(fileName);

					foreach (XmlNode node in doc.DocumentElement.ChildNodes)
					{
						if ("FATURA".Equals(node.Name, StringComparison.CurrentCultureIgnoreCase))
						{
							var fatura = new FaturaEntidade();

							foreach (XmlNode faturaNode in node.ChildNodes)
							{
								if ("DESCRICAO".Equals(faturaNode.Name, StringComparison.CurrentCultureIgnoreCase))
									fatura.Descricao = faturaNode.InnerText;
								else if ("VENCIMENTO".Equals(faturaNode.Name, StringComparison.CurrentCultureIgnoreCase))
									fatura.DataVencimento = DateTime.ParseExact(faturaNode.InnerText, "yyyyMMdd", CultureInfo.InvariantCulture);
								else if ("VALOR".Equals(faturaNode.Name, StringComparison.CurrentCultureIgnoreCase))
									fatura.Valor = Convert.ToDouble(faturaNode.InnerText);
							}

							listaFaturas.Add(fatura);
						}

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
