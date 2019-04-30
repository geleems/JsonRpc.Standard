﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonRpc.Contracts;
using JsonRpc.Messages;
using JsonRpc.Server;

namespace ConsoleTestApp
{
    public class LibraryService : JsonRpcService
    {
        // The service instance is transient. You cannot persist state in such a class.
        // So we need session.
        private LibrarySessionFeature Session => RequestContext.Features.Get<LibrarySessionFeature>();

        [JsonRpcMethod]
        public Book GetBook(string isbn, bool required = false)
        {
            var book = Session.Books.FirstOrDefault(b => IsEqual(b.Isbn, isbn));
            if (required && book == null)
                throw new JsonRpcException(new ResponseError(1000, $"Cannot find book with ISBN:{isbn}."));
            return book;
        }

        [JsonRpcMethod]
        public ResponseError PutBook(Book book)
        {
            // Yes, you can just throw an ordinary Exception… Though it's not recommended.
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (string.IsNullOrEmpty(book.Isbn))
                return new ResponseError(1001, $"Missing Isbn field of the book: {book}.");
            var index = Session.Books.FindIndex(b => IsEqual(b.Isbn, book.Isbn));
            if (index > 0)
                Session.Books[index] = book;
            else
                Session.Books.Add(book);
            return null;
        }

        [JsonRpcMethod]
        public IEnumerable<string> EnumBooksIsbn()
        {
            return Session.Books.Select(b => b.Isbn);
        }

        [JsonRpcMethod(IsNotification = true)]
        public void Terminate()
        {
            Session.StopServer();
        }

        private bool IsEqual(string x, string y)
        {
            if (string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y))
                return string.IsNullOrEmpty(x) && string.IsNullOrEmpty(y);
            int xi = 0, yi = 0;
            NEXT:
            if (xi < x.Length && (x[xi] == '-' || char.IsWhiteSpace(x[xi])))
            {
                xi++;
                goto NEXT;
            }
            if (yi < y.Length && (y[yi] == '-' || char.IsWhiteSpace(y[yi])))
            {
                yi++;
                goto NEXT;
            }
            if (xi == x.Length || yi == y.Length) return xi == x.Length && yi == y.Length;
            if (x[xi] != y[yi]) return false;
            xi++;
            yi++;
            goto NEXT;
        }
    }
}
