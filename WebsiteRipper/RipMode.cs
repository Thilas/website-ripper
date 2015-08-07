namespace WebsiteRipper
{
    public enum RipMode
    {
        /// <summary>
        /// Specifies that <see cref="Ripper"/> should create a new root path.
        /// If the root path already exists, an <see cref="System.IO.IOException"/> exception is thrown.
        /// </summary>
        CreateNew,
        /// <summary>
        /// Specifies that <see cref="Ripper"/> should create a new root path.
        /// If the root path already exists, it will be overwritten.
        /// </summary>
        Create,
        /// <summary>
        /// Specifies that <see cref="Ripper"/> should update an existing root path.
        /// A <see cref="System.IO.DirectoryNotFoundException"/> exception is thrown if the root path does not exist.
        /// </summary>
        Update,
        /// <summary>
        /// Specifies that <see cref="Ripper"/> should update a root path if it exists; otherwise, a new root path should be created.
        /// </summary>
        UpdateOrCreate,
        /// <summary>
        /// Specifies that <see cref="Ripper"/> should truncate and update an existing root path.
        /// A <see cref="System.IO.DirectoryNotFoundException"/> exception is thrown if the root path does not exist.
        /// </summary>
        Truncate
    }
}
