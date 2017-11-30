﻿using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Conduit.Infrastructure.Security;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Users
{
    public class Login
    {
        public class UserData
        {
            public string Email { get; set; }

            public string Password { get; set; }
        }

        public class UserDataValidator : AbstractValidator<UserData>
        {
            public UserDataValidator()
            {
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

        public class Handler : IAsyncRequestHandler<Command, UserEnvelope>
        {
            private readonly ConduitContext _db;
            private readonly IPasswordHasher _passwordHasher;
            private readonly IJwtTokenGenerator _jwtTokenGenerator;
            private readonly IMapper _mapper;

            public Handler(ConduitContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IMapper mapper)
            {
                _db = db;
                _passwordHasher = passwordHasher;
                _jwtTokenGenerator = jwtTokenGenerator;
                _mapper = mapper;
            }

            public async Task<UserEnvelope> Handle(Command message)
            {
                var person = await _db.Persons.Where(x => x.Email == message.User.Email).SingleOrDefaultAsync();
                if (person == null)
                {
                    throw new RestException(HttpStatusCode.Unauthorized);
                }

                if (!person.Hash.SequenceEqual(_passwordHasher.Hash(message.User.Password, person.Salt)))
                {
                    throw new RestException(HttpStatusCode.Unauthorized);
                }
             
                var user  = _mapper.Map<Domain.Person, User>(person); ;
                user.Token = await _jwtTokenGenerator.CreateToken(person.Username);
                return new UserEnvelope(user);
            }
        }
    }
}
