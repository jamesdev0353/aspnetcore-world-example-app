﻿using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Conduit.Domain;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Conduit.Infrastructure.Security;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Users
{
    public class Create
    {
        public class UserData
        {
            public string Username { get; set; }

            public string Email { get; set; }

            public string Password { get; set; }
        }

        public class UserDataValidator : AbstractValidator<UserData>
        {
            public UserDataValidator()
            {
                RuleFor(x => x.Username).NotNull().NotEmpty();
                RuleFor(x => x.Email).NotNull().NotEmpty();
                RuleFor(x => x.Password).NotNull().NotEmpty();
            }
        }

        public class Command : IRequest<UserEnvelope>
        {
            public UserData User { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.User).NotNull().SetValidator(new UserDataValidator());
            }
        }

        public class Handler : IRequestHandler<Command, UserEnvelope>
        {
            private readonly ConduitContext _db;
            private readonly IPasswordHasher _passwordHasher;
            private readonly IMapper _mapper;

            public Handler(ConduitContext db, IPasswordHasher passwordHasher, IMapper mapper)
            {
                _db = db;
                _passwordHasher = passwordHasher;
                _mapper = mapper;
            }

            public async Task<UserEnvelope> Handle(Command message, CancellationToken cancellationToken)
            {
                if (await _db.Persons.Where(x => x.Username == message.User.Username).AnyAsync(cancellationToken))
                {
                    throw new RestException(HttpStatusCode.BadRequest, new { Error = ErrorHelpers.InUse("Username")});
                }

                if (await _db.Persons.Where(x => x.Email == message.User.Email).AnyAsync(cancellationToken))
                {
                    throw new RestException(HttpStatusCode.BadRequest, new { Error = ErrorHelpers.InUse("Email") });
                }

                var salt = Guid.NewGuid().ToByteArray();
                var person = new Person
                {
                    Username = message.User.Username,
                    Email = message.User.Email,
                    Hash = _passwordHasher.Hash(message.User.Password, salt),
                    Salt = salt
                };

                _db.Persons.Add(person);
                await _db.SaveChangesAsync(cancellationToken);

                return new UserEnvelope(_mapper.Map<Domain.Person, User>(person));
            }
        }
    }
}
