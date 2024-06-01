﻿using System.Diagnostics;

namespace FriedChickenScript;

public class Parser
{
    private List<Token> tokens;
    private int tokenIndex = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public ASTNode Parse()
    {
        ASTNode root = new ASTNode(NodeType.Program);
        while (tokenIndex < tokens.Count)
        {
            root.AddChild(ParseStatement());
        }

        return root;
    }

    private ASTNode ParseStatement()
    {
        Token token = GetCurrentToken();

        switch (token.Value)
        {
            case Syntax.Variable:
                return ParseVariableDeclaration();
            case Syntax.Function:
                return ParseFunctionDeclaration();
            case Syntax.Return:
                return ParseFunctionReturn();
            case Syntax.If:
                return ParseIfStatement();
            default:
                return ParseExpression();
        }
    }

    private ASTNode ParseFunctionReturn()
    {
        ASTNode returnNode = new ASTNode(NodeType.ReturnStatement);
        Consume(TokenType.Keyword, Syntax.Return);
        returnNode.AddChild(ParseExpression());
        return returnNode;
    }

    private ASTNode ParseFunctionDeclaration() 
    {
        // func identifier { }
        // func identifier withParams: paramA, paramB { }

        Consume(TokenType.Keyword, Syntax.Function);

        Token name = Consume(TokenType.Identifier);

        ASTNode funcNode = new ASTNode(NodeType.FunctionDeclaration, name.Value);

        if (GetCurrentToken().Value == Syntax.Parameter)
        {
            Consume(TokenType.Keyword, Syntax.Parameter);
            while (GetCurrentToken().Type != TokenType.LeftBrace)
            {
                // 
                Token param = Consume(TokenType.Identifier);
                ASTNode paramNode = new ASTNode(NodeType.Parameter, param.Value);
                if (GetCurrentToken().Value == Syntax.Assignment)
                {
                    Consume(TokenType.Operator, Syntax.Assignment);
                    paramNode.AddChild(new ASTNode(NodeType.Literal, GetCurrentToken().Value));
                    Consume(TokenType.Literal);
                }
                else
                {
                    paramNode.AddChild(new ASTNode(NodeType.Literal, null));
                }

                funcNode.AddChild(paramNode);

                if (GetCurrentToken().Type == TokenType.Delimiter)
                {
                    Consume(TokenType.Delimiter);
                }
            }
        }

        Consume(TokenType.LeftBrace);
        funcNode.AddChild(ParseBlock());
        Consume(TokenType.RightBrace);
        return funcNode;
    }

    private ASTNode ParseBlock()
    {
        ASTNode blockNode = new ASTNode(NodeType.Block);

        while (GetCurrentToken().Type != TokenType.RightBrace && tokenIndex < tokens.Count)
        {
            blockNode.AddChild(ParseStatement());
        }

        return blockNode;
    }


    private ASTNode ParseVariableDeclaration()
    {
        Consume(TokenType.Keyword, Syntax.Variable);

        Token name = Consume(TokenType.Identifier);
        Consume(TokenType.Operator, Syntax.Assignment);

        ASTNode varNode = new ASTNode(NodeType.VariableDeclaration, name.Value);

        varNode.AddChild(ParseExpression());

        return varNode;
    }
    private ASTNode ParseExpression()
    {
        ASTNode parsed = ParsePrimary();
        parsed = ParseMultDiv(parsed);
        parsed = ParseAddSub(parsed);
        parsed = ParseComparison(parsed);
        return parsed;
    }

    private ASTNode ParseAddSub(ASTNode current)
    {
        ASTNode node = current;

        while (GetCurrentToken()?.Type == TokenType.Operator
           && (GetCurrentToken()?.Value == Syntax.Addition || GetCurrentToken()?.Value == Syntax.Subtraction))
        {
            Token operatorToken = Consume(TokenType.Operator, GetCurrentToken().Value);
            ASTNode right = ParseExpression();
            ASTNode binaryExpression = new ASTNode(NodeType.BinaryExpression, operatorToken.Value);
            binaryExpression.AddChild(node);
            binaryExpression.AddChild(right);
            node = binaryExpression;
        }

        return node;
    }

    private ASTNode ParseMultDiv(ASTNode current)
    {
        ASTNode node = current;

        while (GetCurrentToken()?.Type == TokenType.Operator
           && (GetCurrentToken()?.Value == Syntax.Multiplication) || GetCurrentToken()?.Value == Syntax.Division)
        {
            Token operatorToken = Consume(TokenType.Operator, GetCurrentToken().Value);
            ASTNode right = ParsePrimary();
            ASTNode binaryExpression = new ASTNode(NodeType.BinaryExpression, operatorToken.Value);
            binaryExpression.AddChild(node);
            binaryExpression.AddChild(right);
            node = binaryExpression;
        }

        return node;
    }

    private ASTNode ParseComparison(ASTNode current)
    {
        ASTNode node = current;

        while (GetCurrentToken()?.Type == TokenType.Operator &&
               (GetCurrentToken()?.Value == Syntax.LessThan || GetCurrentToken()?.Value == Syntax.GreaterThan ||
                GetCurrentToken()?.Value == "<=" || GetCurrentToken()?.Value == ">=" ||
                GetCurrentToken()?.Value == "==" || GetCurrentToken()?.Value == "!="))
        {
            Token operatorToken = Consume(TokenType.Operator);
            ASTNode right = ParsePrimary();
            ASTNode comparisonExpression = new ASTNode(NodeType.BinaryExpression, operatorToken.Value);
            comparisonExpression.AddChild(node);
            comparisonExpression.AddChild(right);
            node = comparisonExpression;
        }

        return node;
    }

    private ASTNode ParsePrimary()
    {
        Token currentToken = GetCurrentToken();

        if (currentToken.Type == TokenType.Literal)
        {
            Consume(TokenType.Literal);
            return new ASTNode(NodeType.Literal, currentToken.Value);
        }
        else if (currentToken.Type == TokenType.Identifier)
        {
            Token identifierToken = Consume(TokenType.Identifier);
            if (GetCurrentToken()?.Type == TokenType.LeftParen)
            {
                return ParseFunctionCall(identifierToken);
            }
            return new ASTNode(NodeType.Identifier, identifierToken.Value);
        }
        else if (currentToken.Type == TokenType.LeftParen)
        {
            Consume(TokenType.LeftParen, "(");
            ASTNode expression = ParseExpression();
            Consume(TokenType.RightParen, ")");
            return expression;
        }
        else
        {
            throw new Exception("Unexpected token: " + currentToken.Type);
        }
    }

    private ASTNode ParseFunctionCall(Token functionNameToken)
    {
        ASTNode funcCallNode = new ASTNode(NodeType.FunctionCall, functionNameToken.Value);
        Consume(TokenType.LeftParen);

        ASTNode args = new ASTNode(NodeType.Arguments);
        while (GetCurrentToken().Type != TokenType.RightParen)
        {
            ASTNode argument = ParseExpression();
            args.AddChild(argument);

            if (GetCurrentToken().Type == TokenType.Delimiter)
                Consume(TokenType.Delimiter);
            else
                break;
        }
        if (args.Children.Count > 0)
        {
            funcCallNode.AddChild(args);
        }
        Consume(TokenType.RightParen);
        return funcCallNode;
    }

    private ASTNode ParseIfStatement()
    {
        Consume(TokenType.Keyword, Syntax.If);

        ASTNode ifNode = new ASTNode(NodeType.IfStatement);

        // Parse condition
        Consume(TokenType.LeftParen);
        ifNode.AddChild(ParseExpression());
        Consume(TokenType.RightParen);

        // Parse if block
        Consume(TokenType.LeftBrace);
        ifNode.AddChild(ParseBlock());
        Consume(TokenType.RightBrace);

        // Check for else block
        if (GetCurrentToken()?.Value == Syntax.Else)
        {
            Consume(TokenType.Keyword, Syntax.Else);

            // Parse else block
            Consume(TokenType.LeftBrace);
            ifNode.AddChild(ParseBlock());
            Consume(TokenType.RightBrace);
        }

        return ifNode;
    }


    private Token Consume(TokenType type, string value = null)
    {
        Token token = tokens[tokenIndex];
        if (token.Type != type || (value != null && token.Value != value))
        {
            throw new Exception($"Expected {type} {value}, but got {token.Type} '{token.Value}'");
        }
        tokenIndex++;
        return token;
    }

    private Token GetCurrentToken() 
    {
        if (tokenIndex < tokens.Count)
            return tokens[tokenIndex];

        return null;
    }
}