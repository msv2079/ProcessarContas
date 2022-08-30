namespace ProcessarContas.Logica
{
	public interface IFatura
	{
		public string GetExtensaoArquivo();
		public bool Processar(string fileName, int nrRegistrosProcessar);
	}
}
